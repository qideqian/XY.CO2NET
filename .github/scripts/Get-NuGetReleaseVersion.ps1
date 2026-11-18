#requires -Version 7.0
<#
.SYNOPSIS
根据当前发布线、Git 标签和 NuGet 已发布版本计算下一次发布版本。
.PARAMETER ConfigPath
发布配置文件，包含 versionPrefix、minimumVersion 和 projects。
.PARAMETER RepositoryPath
已获取完整远端标签的仓库目录；脚本只读 Git 状态，不创建或推送标签。
.PARAMETER Commit
准备发布的提交，默认使用检出的 HEAD。
.OUTPUTS
包含 Version（包版本）、Tag（标签名）、Commit（完整提交号）和 ReusedTag（是否重试）的对象。
#>
[CmdletBinding()]
param(
    [string] $ConfigPath = (Join-Path $PSScriptRoot '../nuget-release.json'),
    [string] $RepositoryPath = (Join-Path $PSScriptRoot '../..'),
    [string] $Commit = 'HEAD'
)

function Get-ReleasePatch {
    <#
    .SYNOPSIS
    提取指定主次版本线上的正式补丁号；其他版本线、预发布和非规范版本返回空值。
    .PARAMETER Value
    不含 v 前缀的候选 NuGet 版本。
    .PARAMETER VersionPrefix
    需要匹配的主次版本，例如 2.0。
    #>
    param([string] $Value, [string] $VersionPrefix)

    if ($Value -cmatch ('^' + [regex]::Escape($VersionPrefix) + '\.(0|[1-9][0-9]*)$')) {
        $patch = 0
        if (-not [int]::TryParse($Matches[1], [ref] $patch)) {
            throw "版本补丁号超出整数范围：$Value"
        }
        return $patch
    }
}

function Select-NuGetReleaseVersion {
    <#
    .SYNOPSIS
    纯计算版本选择，不访问网络或修改仓库，供发布入口和回归测试共同使用。
    .PARAMETER VersionPrefix
    固定的主次版本线；不会受历史上其他版本线的标签影响。
    .PARAMETER MinimumVersion
    当前发布线允许生成的最低版本。
    .PARAMETER Tags
    仓库的全部标签名。
    .PARAMETER CommitTags
    指向本次提交的标签名；同一版本线只能有一个正式标签。
    .PARAMETER PublishedVersions
    所有待发布包在 NuGet 上已经占用的版本号。
    .OUTPUTS
    Version、Tag、ReusedTag；重跑已打标签的提交时复用原版本，不再次递增。
    #>
    param(
        [Parameter(Mandatory)] [string] $VersionPrefix,
        [Parameter(Mandatory)] [string] $MinimumVersion,
        [string[]] $Tags = @(),
        [string[]] $CommitTags = @(),
        [string[]] $PublishedVersions = @()
    )

    if ($VersionPrefix -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
        throw "发布线必须为规范的主版本.次版本：$VersionPrefix"
    }
    $minimumPatch = Get-ReleasePatch -Value $MinimumVersion -VersionPrefix $VersionPrefix
    if ($null -eq $minimumPatch) {
        throw "最低版本 $MinimumVersion 不属于正式发布线 $VersionPrefix。"
    }

    $matchingCommitTags = @($CommitTags | Where-Object {
        $_.StartsWith('v', [System.StringComparison]::Ordinal) -and
        $null -ne (Get-ReleasePatch -Value $_.Substring(1) -VersionPrefix $VersionPrefix)
    } | Sort-Object -Unique)
    if ($matchingCommitTags.Count -gt 1) {
        throw "当前提交存在多个正式发布标签，无法确定重试版本：$($matchingCommitTags -join ', ')"
    }
    if ($matchingCommitTags.Count -eq 1) {
        return [pscustomobject]@{
            Version = $matchingCommitTags[0].Substring(1)
            Tag = $matchingCommitTags[0]
            ReusedTag = $true
        }
    }

    $maximumPatch = -1
    $candidates = @($PublishedVersions) + @($Tags | Where-Object {
        $_.StartsWith('v', [System.StringComparison]::Ordinal)
    } | ForEach-Object { $_.Substring(1) })
    foreach ($candidate in $candidates) {
        $candidatePatch = Get-ReleasePatch -Value $candidate -VersionPrefix $VersionPrefix
        if ($null -ne $candidatePatch -and $candidatePatch -gt $maximumPatch) {
            $maximumPatch = $candidatePatch
        }
    }
    if ($maximumPatch -eq [int]::MaxValue) {
        throw '补丁号已达到上限，请调整发布线后再发布。'
    }

    $nextPatch = [Math]::Max($minimumPatch, $maximumPatch + 1)
    $version = "$VersionPrefix.$nextPatch"
    return [pscustomobject]@{
        Version = $version
        Tag = "v$version"
        ReusedTag = $false
    }
}

function Invoke-ReleaseGit {
    <#
    .SYNOPSIS
    在指定仓库执行只读 Git 命令；任何非零退出码都终止版本计算。
    .PARAMETER RepositoryPath
    Git 仓库目录。
    .PARAMETER Arguments
    作为独立参数传递的 Git 命令，避免通过拼接 shell 执行。
    #>
    param([string] $RepositoryPath, [string[]] $Arguments)

    $PSNativeCommandUseErrorActionPreference = $false
    $result = @(& git -C $RepositoryPath @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "读取 Git 状态失败：$($result -join [Environment]::NewLine)"
    }
    return $result
}

function Get-NuGetPublishedVersions {
    <#
    .SYNOPSIS
    查询 NuGet 官方源中的所有已发布版本；仅 404 表示包尚不存在。
    .PARAMETER PackageId
    待发布的 NuGet 包标识；网络错误或无效响应必须终止，防止误用已有版本。
    #>
    param([Parameter(Mandatory)] [string] $PackageId)

    $uri = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLowerInvariant())/index.json"
    try {
        $response = Invoke-RestMethod -Uri $uri -Method Get -TimeoutSec 60 -ErrorAction Stop
    }
    catch {
        if ($null -ne $_.Exception.Response -and [int] $_.Exception.Response.StatusCode -eq 404) {
            Write-Host "NuGet 包尚不存在：$PackageId"
            return
        }
        throw "查询 NuGet 包 $PackageId 失败，停止版本计算：$($_.Exception.Message)"
    }
    if ($null -eq $response -or $null -eq $response.versions -or $response.versions -is [string]) {
        throw "NuGet 包 $PackageId 返回的版本列表无效。"
    }
    foreach ($version in $response.versions) {
        if ($version -isnot [string] -or [string]::IsNullOrWhiteSpace($version)) {
            throw "NuGet 包 $PackageId 返回了无效的版本条目。"
        }
        $version
    }
}

function Get-NuGetReleaseVersion {
    <#
    .SYNOPSIS
    读取配置和 Git 状态，查询线上已用版本并返回唯一发布计划。
    .PARAMETER ConfigPath
    发布配置 JSON 文件路径。
    .PARAMETER RepositoryPath
    待发布的本地仓库目录。
    .PARAMETER Commit
    待发布提交的 Git 修订引用；解析后返回完整提交号。
    #>
    param([string] $ConfigPath, [string] $RepositoryPath, [string] $Commit = 'HEAD')

    $ErrorActionPreference = 'Stop'
    $config = Get-Content -LiteralPath $ConfigPath -Raw -Encoding utf8 | ConvertFrom-Json
    if ($null -eq $config.projects -or @($config.projects).Count -eq 0) {
        throw '发布配置中没有待发布项目。'
    }
    $packageIds = @($config.projects | ForEach-Object {
        $projectPath = Join-Path $RepositoryPath $_
        if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
            throw "发布项目不存在：$projectPath"
        }
        # 当前仓库的包标识与项目文件名相同；只接受合法包名，避免构造意外请求路径。
        $packageId = [IO.Path]::GetFileNameWithoutExtension($_)
        if ($packageId -cnotmatch '^[A-Za-z0-9_.-]+$') {
            throw "无效的 NuGet 包标识：$packageId"
        }
        $packageId
    })
    if (@($packageIds | Sort-Object -Unique).Count -ne $packageIds.Count) {
        throw '发布项目包含重复的 NuGet 包标识。'
    }

    $resolved = @(Invoke-ReleaseGit -RepositoryPath $RepositoryPath -Arguments @('rev-parse', '--verify', "$Commit^{commit}"))
    if ($resolved.Count -ne 1 -or $resolved[0] -cnotmatch '^[0-9a-f]{40,64}$') {
        throw "无法解析待发布提交：$Commit"
    }
    $resolvedCommit = [string] $resolved[0]
    $tags = @(Invoke-ReleaseGit -RepositoryPath $RepositoryPath -Arguments @('tag', '--list'))
    $commitTags = @(Invoke-ReleaseGit -RepositoryPath $RepositoryPath -Arguments @('tag', '--points-at', $resolvedCommit))
    $selectionParameters = @{
        VersionPrefix = [string] $config.versionPrefix
        MinimumVersion = [string] $config.minimumVersion
        Tags = $tags
        CommitTags = $commitTags
    }
    $selection = Select-NuGetReleaseVersion @selectionParameters
    if (-not $selection.ReusedTag) {
        $publishedVersions = @($packageIds | ForEach-Object { Get-NuGetPublishedVersions -PackageId $_ })
        $selection = Select-NuGetReleaseVersion @selectionParameters -PublishedVersions $publishedVersions
    }

    Write-Host "发布版本：$($selection.Version)，标签：$($selection.Tag)，复用标签：$($selection.ReusedTag)"
    # 发布计划是脚本唯一的成功输出，便于工作流可靠地写入 GITHUB_OUTPUT。
    return [pscustomobject]@{
        Version = $selection.Version
        Tag = $selection.Tag
        Commit = $resolvedCommit
        ReusedTag = $selection.ReusedTag
    }
}

# 点加载仅导入函数，供离线回归测试使用；直接执行才查询真实版本。
if ($MyInvocation.InvocationName -ne '.') {
    Get-NuGetReleaseVersion -ConfigPath $ConfigPath -RepositoryPath $RepositoryPath -Commit $Commit
}

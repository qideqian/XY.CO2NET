#requires -Version 7.0
<#
.SYNOPSIS
离线验证版本递增、失败关闭和重试行为；仅在临时目录创建 Git 仓库。
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Get-NuGetReleaseVersion.ps1')
$script:PassedCount = 0
$script:ResponseMode = 'success'
$script:PublishedVersions = @('2.0.8', '2.0.9', '20.0.4')
$script:RequestCount = 0

function Assert-Equal {
    <# .SYNOPSIS 检查实际结果与预期一致，否则中止测试并显示用例名称。 #>
    param($Actual, $Expected, [string] $Name)
    if ($Actual -cne $Expected) {
        throw "$Name：预期 [$Expected]，实际 [$Actual]。"
    }
    $script:PassedCount++
    Write-Host "通过：$Name"
}

function Assert-Throws {
    <# .SYNOPSIS 验证指定操作必须失败，且错误信息符合预期。 #>
    param([scriptblock] $Action, [string] $ExpectedMessage, [string] $Name)
    $caught = $null
    try { & $Action | Out-Null } catch { $caught = $_ }
    if ($null -eq $caught -or $caught.Exception.Message -notlike $ExpectedMessage) {
        throw "$Name：未得到预期错误 [$ExpectedMessage]；实际为 [$caught]。"
    }
    $script:PassedCount++
    Write-Host "通过：$Name"
}

function Invoke-RestMethod {
    <# .SYNOPSIS 替换真实 NuGet HTTP 调用，覆盖成功、404、服务故障和无效响应。 #>
    param([string] $Uri, [string] $Method, [int] $TimeoutSec, [string] $ErrorAction)
    $script:RequestCount++
    if ($Uri -notmatch '^https://api\.nuget\.org/v3-flatcontainer/test\.package/index\.json$') {
        throw "测试遇到非预期请求：$Uri"
    }
    switch ($script:ResponseMode) {
        'success' { return [pscustomobject]@{ versions = $script:PublishedVersions } }
        'malformed' { return [pscustomobject]@{ error = '没有版本列表' } }
        'network' { throw [System.Net.Http.HttpRequestException]::new('模拟连接失败') }
        'notfound' {
            $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]::NotFound)
            throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('模拟 404', $response)
        }
        'unavailable' {
            $response = [System.Net.Http.HttpResponseMessage]::new([System.Net.HttpStatusCode]::ServiceUnavailable)
            throw [Microsoft.PowerShell.Commands.HttpResponseException]::new('模拟 503', $response)
        }
        default { throw '测试响应模式未定义。' }
    }
}

$base = @{ VersionPrefix = '2.0'; MinimumVersion = '2.0.10' }
Assert-Equal (Select-NuGetReleaseVersion @base).Version '2.0.10' '首次发布使用最低版本'
Assert-Equal (Select-NuGetReleaseVersion @base -Tags @('v2.0.9', 'v2.0.10')).Version '2.0.11' '补丁号按整数排序'
Assert-Equal (Select-NuGetReleaseVersion @base -Tags @('v2.0.9') -PublishedVersions @('2.0.12')).Version '2.0.13' '线上版本高于标签时避开已用版本'
Assert-Equal (Select-NuGetReleaseVersion @base -Tags @('v20.0.4', 'v2.0.09', 'V2.0.99', 'v2.0.99-beta', 'v2.0.99+build') -PublishedVersions @('20.0.4', '2.0.99-rc.1', '2.0.99+build')).Version '2.0.10' '忽略其他发布线和非规范正式版本'
$retry = Select-NuGetReleaseVersion @base -Tags @('v2.0.10', 'v2.0.20') -CommitTags @('v2.0.10', 'v20.0.4') -PublishedVersions @('2.0.20')
Assert-Equal $retry.Version '2.0.10' '旧提交重试保持原版本'
Assert-Equal $retry.ReusedTag $true '重试标记准确'
Assert-Equal (Select-NuGetReleaseVersion -VersionPrefix '2.0' -MinimumVersion '2.0.30' -Tags @('v2.0.20')).Version '2.0.30' '提高最低版本不会降回历史补丁'
Assert-Throws { Select-NuGetReleaseVersion @base -CommitTags @('v2.0.10', 'v2.0.11') } '*多个正式发布标签*' '拒绝同一提交的歧义标签'
Assert-Throws { Select-NuGetReleaseVersion -VersionPrefix '02.0' -MinimumVersion '02.0.10' } '*规范的主版本*' '拒绝带前导零的发布线'
Assert-Throws { Select-NuGetReleaseVersion -VersionPrefix '2.0' -MinimumVersion '3.0.0' } '*不属于正式发布线*' '拒绝跨线最低版本'
Assert-Throws { Select-NuGetReleaseVersion @base -Tags @('v2.0.2147483647') } '*达到上限*' '补丁号溢出时停止'

$temporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixturePath = Join-Path $temporaryBase ('xy-nuget-version-tests-' + [Guid]::NewGuid().ToString('N'))
try {
    New-Item -ItemType Directory -Path (Join-Path $fixturePath '.github') -Force | Out-Null
    $fixtureConfig = Join-Path $fixturePath '.github/nuget-release.json'
    [ordered]@{ versionPrefix = '2.0'; minimumVersion = '2.0.10'; projects = @('Test.Package.csproj') } |
        ConvertTo-Json | Set-Content -LiteralPath $fixtureConfig -Encoding utf8
    '<Project Sdk="Microsoft.NET.Sdk" />' | Set-Content -LiteralPath (Join-Path $fixturePath 'Test.Package.csproj') -Encoding utf8
    Invoke-ReleaseGit $fixturePath @('init', '--quiet') | Out-Null
    Invoke-ReleaseGit $fixturePath @('config', 'user.name', 'NuGet 版本回归测试') | Out-Null
    Invoke-ReleaseGit $fixturePath @('config', 'user.email', 'nuget-tests@example.invalid') | Out-Null
    Invoke-ReleaseGit $fixturePath @('add', '.') | Out-Null
    Invoke-ReleaseGit $fixturePath @('-c', 'commit.gpgsign=false', 'commit', '--quiet', '-m', '历史版本') | Out-Null
    Invoke-ReleaseGit $fixturePath @('-c', 'tag.gpgsign=false', 'tag', '-a', 'v2.0.9', '-m', '历史正式标签') | Out-Null
    Invoke-ReleaseGit $fixturePath @('tag', 'v20.0.4') | Out-Null
    Invoke-ReleaseGit $fixturePath @('-c', 'commit.gpgsign=false', 'commit', '--quiet', '--allow-empty', '-m', '准备自动发布') | Out-Null

    $parameters = @{ ConfigPath = $fixtureConfig; RepositoryPath = $fixturePath; Commit = 'HEAD' }
    $plan = @(Get-NuGetReleaseVersion @parameters)
    Assert-Equal $plan.Count 1 '入口仅返回一个发布计划对象'
    Assert-Equal $plan[0].Version '2.0.10' '真实 Git 仓库结合 NuGet 响应选出首次版本'
    Assert-Equal $plan[0].Commit ([string] (Invoke-ReleaseGit $fixturePath @('rev-parse', 'HEAD'))) '标签目标为检出的完整提交号'
    Assert-Equal $plan[0].ReusedTag $false '新提交不标记为重试'

    $script:PublishedVersions = @('2.0.15', '20.0.4')
    Assert-Equal (Get-NuGetReleaseVersion @parameters).Version '2.0.16' '无标签的线上既有版本仍参与递增'
    $script:ResponseMode = 'notfound'
    Assert-Equal (Get-NuGetReleaseVersion @parameters).Version '2.0.10' 'NuGet 404 允许首次发布'
    $script:ResponseMode = 'unavailable'
    Assert-Throws { Get-NuGetReleaseVersion @parameters } '*查询 NuGet 包*失败*' 'NuGet 503 不得作为空版本列表'
    $script:ResponseMode = 'network'
    Assert-Throws { Get-NuGetReleaseVersion @parameters } '*模拟连接失败*' '网络连接异常必须停止'
    $script:ResponseMode = 'malformed'
    Assert-Throws { Get-NuGetReleaseVersion @parameters } '*版本列表无效*' '无效 JSON 结构必须停止'

    Invoke-ReleaseGit $fixturePath @('-c', 'tag.gpgsign=false', 'tag', '-a', 'v2.0.10', '-m', '已保留的发布版本') | Out-Null
    $requestsBeforeRetry = $script:RequestCount
    $retryPlan = Get-NuGetReleaseVersion @parameters
    Assert-Equal $retryPlan.Version '2.0.10' '已打标签的提交复用版本'
    Assert-Equal $retryPlan.ReusedTag $true '识别真实 Git 的注释标签'
    Assert-Equal $script:RequestCount $requestsBeforeRetry '重试不依赖 NuGet 版本查询可用性'
    Invoke-ReleaseGit $fixturePath @('tag', 'v2.0.11') | Out-Null
    Assert-Throws { Get-NuGetReleaseVersion @parameters } '*多个正式发布标签*' '真实 Git 同提交多标签拒绝发布'
    Assert-Throws { Get-NuGetReleaseVersion -ConfigPath $fixtureConfig -RepositoryPath $fixturePath -Commit 'missing-commit' } '*读取 Git 状态失败*' '不存在的提交必须停止'
}
finally {
    # 仅清理本次生成的临时夹具，删除前核验绝对路径仍处于系统临时目录。
    if (Test-Path -LiteralPath $fixturePath) {
        $resolvedFixture = (Resolve-Path -LiteralPath $fixturePath).Path
        $expectedPrefix = Join-Path $temporaryBase 'xy-nuget-version-tests-'
        if (-not $resolvedFixture.StartsWith($expectedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw "拒绝清理临时目录之外的路径：$resolvedFixture"
        }
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}

Write-Host "NuGet 版本回归测试通过：$script:PassedCount 项。"

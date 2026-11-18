<#
.SYNOPSIS
使用统一版本还原、构建、测试并检查全部待发布 NuGet 包，不创建标签或上传包。
.PARAMETER Version
本次发布的三段式正式版本，必须由版本计算步骤确定。
.PARAMETER OutputDirectory
包输出目录；必须为空，防止把旧包混入本次发布。
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$')]
    [string] $Version,
    [string] $OutputDirectory = 'artifacts/nuget'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-CheckedDotNet {
    <# 执行 .NET CLI；任何非零退出码都终止发布，不能被后续成功命令覆盖。 #>
    param([Parameter(Mandatory)][string[]] $Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments[0]) 失败，退出码：$LASTEXITCODE"
    }
}

$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repositoryRoot
try {
    # 发布清单按依赖顺序排列，核心包在依赖它的 Redis、SMS 包之前。
    $config = Get-Content '.github/nuget-release.json' -Raw -Encoding utf8 | ConvertFrom-Json
    $outputPath = [IO.Path]::GetFullPath($OutputDirectory)
    if ((Test-Path -LiteralPath $outputPath) -and @(Get-ChildItem -LiteralPath $outputPath -Force).Count -gt 0) {
        throw "输出目录必须为空，请指定新目录：$outputPath"
    }
    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

    # 同时覆盖程序集版本和包版本，保证 --no-build 打包以及项目间依赖使用同一版本。
    $properties = @(
        "-p:Version=$Version",
        "-p:PackageVersion=$Version",
        '-p:ContinuousIntegrationBuild=true',
        '-p:GeneratePackageOnBuild=false'
    )
    Invoke-CheckedDotNet (@('restore', 'XY.CO2NET.sln', '--nologo') + $properties)
    Invoke-CheckedDotNet (@('build', 'XY.CO2NET.sln', '-c', 'Release', '--no-restore', '--nologo') + $properties)
    # 保留默认 VSTest 模式，兼容 net48 的 MSTest 3 与 net10 的 MSTest 4 项目。
    Invoke-CheckedDotNet (@('test', 'XY.CO2NET.sln', '-c', 'Release', '--no-build', '--no-restore', '--nologo') + $properties)

    foreach ($project in $config.projects) {
        Invoke-CheckedDotNet (@('pack', $project, '-c', 'Release', '--no-build', '--no-restore', '--nologo', '-o', $outputPath) + $properties)
    }

    $packageIds = @($config.projects | ForEach-Object { [IO.Path]::GetFileNameWithoutExtension($_) })
    $packages = @(Get-ChildItem -LiteralPath $outputPath -Filter '*.nupkg' -File)
    if ($packages.Count -ne $packageIds.Count) {
        throw "预期 $($packageIds.Count) 个 NuGet 包，实际生成 $($packages.Count) 个。"
    }

    foreach ($project in $config.projects) {
        $packageId = [IO.Path]::GetFileNameWithoutExtension($project)
        $packagePath = Join-Path $outputPath "$packageId.$Version.nupkg"
        $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
        try {
            $specifications = @($archive.Entries | Where-Object { $_.FullName -like '*.nuspec' })
            if ($specifications.Count -ne 1) { throw "$packageId 缺少唯一的 nuspec 清单。" }
            $reader = [IO.StreamReader]::new($specifications[0].Open())
            try { [xml] $specification = $reader.ReadToEnd() } finally { $reader.Dispose() }
            $metadata = $specification.package.metadata
            if ($metadata.id -ne $packageId -or $metadata.version -ne $Version) {
                throw "$packageId 的包标识或版本与本次发布不一致。"
            }
            # 检查每个目标框架均有主程序集，防止空包或遗漏某个框架。
            [xml] $projectXml = Get-Content -LiteralPath $project -Raw -Encoding utf8
            $frameworks = $projectXml.SelectSingleNode('//TargetFrameworks').InnerText -split ';'
            foreach ($framework in $frameworks) {
                if ($null -eq $archive.GetEntry("lib/$framework/$packageId.dll")) {
                    throw "$packageId 缺少 $framework 的程序集。"
                }
            }
            # 包间依赖须指向同一批次版本，不能残留项目文件中的旧版本。
            foreach ($dependency in $specification.SelectNodes('//*[local-name()="dependency"]')) {
                if ($dependency.id -in $packageIds -and $dependency.version -ne $Version) {
                    throw "$packageId 的内部依赖 $($dependency.id) 版本错误：$($dependency.version)"
                }
            }
            $symbolFormat = $projectXml.SelectSingleNode('//SymbolPackageFormat')
            if ($null -ne $symbolFormat -and $symbolFormat.InnerText -eq 'snupkg') {
                if (-not (Test-Path -LiteralPath (Join-Path $outputPath "$packageId.$Version.snupkg"))) {
                    throw "$packageId 缺少符号包。"
                }
            }
            Write-Host "包检查通过：$packageId $Version，框架：$($frameworks -join ', ')"
        }
        finally { $archive.Dispose() }
    }
    Write-Host "发布前验证完成：$($packages.Count) 个包；输出目录：$outputPath"
}
finally { Pop-Location }

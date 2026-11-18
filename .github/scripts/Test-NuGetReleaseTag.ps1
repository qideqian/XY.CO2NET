#requires -Version 7.0
<#
.SYNOPSIS
在临时本地 Git 仓库中验证发布标签的创建、重试和冲突保护；不会访问真实远端。
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$publishScript = Join-Path $PSScriptRoot 'Publish-NuGetReleaseTag.ps1'
$script:PassedCount = 0

function Invoke-TestGit {
    <# .SYNOPSIS 执行测试夹具中的 Git 操作，非零退出码立即终止测试。 #>
    param([string] $Repository, [string[]] $Arguments)
    $PSNativeCommandUseErrorActionPreference = $false
    $result = @(& git -C $Repository @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "测试 Git 操作失败：$($result -join [Environment]::NewLine)"
    }
    return $result
}

function Assert-TagEqual {
    <# .SYNOPSIS 检查标签对象、目标提交或错误后的状态，避免静默覆盖发布记录。 #>
    param($Actual, $Expected, [string] $Name)
    if ($Actual -cne $Expected) { throw "$Name：预期 [$Expected]，实际 [$Actual]。" }
    $script:PassedCount++
    Write-Host "通过：$Name"
}

function Assert-TagFailure {
    <# .SYNOPSIS 检查发布标签操作按预期拒绝，并验证返回的错误类别。 #>
    param([scriptblock] $Action, [string] $ExpectedMessage, [string] $Name)
    $caught = $null
    try { & $Action | Out-Null } catch { $caught = $_ }
    if ($null -eq $caught -or $caught.Exception.Message -notlike $ExpectedMessage) {
        throw "$Name：预期错误 [$ExpectedMessage]，实际 [$caught]。"
    }
    $script:PassedCount++
    Write-Host "通过：$Name"
}

$temporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureRoot = Join-Path $temporaryBase ('xy-nuget-tag-tests-' + [Guid]::NewGuid().ToString('N'))
$workingRepository = Join-Path $fixtureRoot 'working'
$bareRepository = Join-Path $fixtureRoot 'remote.git'
$locationPushed = $false
try {
    New-Item -ItemType Directory -Path $workingRepository -Force | Out-Null
    New-Item -ItemType Directory -Path $bareRepository -Force | Out-Null
    Invoke-TestGit $bareRepository @('init', '--bare', '--quiet') | Out-Null
    Invoke-TestGit $workingRepository @('init', '--quiet') | Out-Null
    Invoke-TestGit $workingRepository @('config', 'user.name', 'NuGet 标签回归测试') | Out-Null
    Invoke-TestGit $workingRepository @('config', 'user.email', 'nuget-tag-tests@example.invalid') | Out-Null
    # 排除开发机全局签名及钩子配置影响；测试远端仅指向本次创建的本地裸仓库。
    Invoke-TestGit $workingRepository @('config', 'commit.gpgsign', 'false') | Out-Null
    Invoke-TestGit $workingRepository @('config', 'tag.gpgsign', 'false') | Out-Null
    $emptyHooks = Join-Path $fixtureRoot 'empty-hooks'
    New-Item -ItemType Directory -Path $emptyHooks -Force | Out-Null
    Invoke-TestGit $workingRepository @('config', 'core.hooksPath', $emptyHooks) | Out-Null
    Invoke-TestGit $bareRepository @('config', 'core.hooksPath', $emptyHooks) | Out-Null
    Invoke-TestGit $workingRepository @('remote', 'add', 'fixture', $bareRepository) | Out-Null
    Invoke-TestGit $workingRepository @('commit', '--quiet', '--allow-empty', '-m', '首次发布提交') | Out-Null
    $firstCommit = [string](Invoke-TestGit $workingRepository @('rev-parse', 'HEAD'))
    Push-Location $workingRepository
    $locationPushed = $true

    & $publishScript -Tag 'v2.0.10' -Commit $firstCommit -Remote 'fixture'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('cat-file', '-t', 'refs/tags/v2.0.10'))) 'tag' '新发布创建注释标签'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'v2.0.10^{commit}'))) $firstCommit '远程标签指向实际构建提交'
    $originalTagObject = [string](Invoke-TestGit $bareRepository @('rev-parse', 'refs/tags/v2.0.10'))

    & $publishScript -Tag 'v2.0.10' -Commit $firstCommit -Remote 'fixture'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'refs/tags/v2.0.10'))) $originalTagObject '重试复用同一远程标签对象'

    # 模拟推送中断后本地标签已存在、远端尚不存在的恢复路径。
    Invoke-TestGit $workingRepository @('tag', '-a', 'v2.0.11', '-m', '已保留但尚未推送') | Out-Null
    $reservedTagObject = [string](Invoke-TestGit $workingRepository @('rev-parse', 'refs/tags/v2.0.11'))
    & $publishScript -Tag 'v2.0.11' -Commit $firstCommit -Remote 'fixture'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'refs/tags/v2.0.11'))) $reservedTagObject '重试推送已有本地注释标签'

    # 历史轻量标签仍可复用，但不会转换为新注释标签或重写对象。
    Invoke-TestGit $bareRepository @('tag', 'v2.0.12', $firstCommit) | Out-Null
    & $publishScript -Tag 'v2.0.12' -Commit $firstCommit -Remote 'fixture'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'refs/tags/v2.0.12'))) $firstCommit '同提交远程轻量标签可复用'

    Invoke-TestGit $workingRepository @('commit', '--quiet', '--allow-empty', '-m', '第二次发布提交') | Out-Null
    $secondCommit = [string](Invoke-TestGit $workingRepository @('rev-parse', 'HEAD'))
    Assert-TagFailure { & $publishScript -Tag 'v2.0.13' -Commit $firstCommit -Remote 'fixture' } '*HEAD*不一致*' '拒绝与工作区 HEAD 不一致的提交'
    Assert-TagEqual (@(Invoke-TestGit $bareRepository @('tag', '--list', 'v2.0.13')).Count) 0 '错误提交不会创建远程标签'

    Assert-TagFailure { & $publishScript -Tag 'v2.0.10' -Commit $secondCommit -Remote 'fixture' } '*远程标签*禁止覆盖*' '拒绝覆盖其他提交的远程注释标签'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'refs/tags/v2.0.10'))) $originalTagObject '远程冲突后保留原标签对象'
    Assert-TagFailure { & $publishScript -Tag 'v2.0.12' -Commit $secondCommit -Remote 'fixture' } '*远程标签*禁止覆盖*' '拒绝覆盖其他提交的远程轻量标签'

    Invoke-TestGit $workingRepository @('tag', 'v2.0.14', $firstCommit) | Out-Null
    Assert-TagFailure { & $publishScript -Tag 'v2.0.14' -Commit $secondCommit -Remote 'fixture' } '*本地标签*禁止覆盖*' '拒绝移动已指向其他提交的本地标签'
    Assert-TagEqual (@(Invoke-TestGit $bareRepository @('tag', '--list', 'v2.0.14')).Count) 0 '本地冲突不会创建远程标签'

    $missingRemote = Join-Path $fixtureRoot 'missing.git'
    Assert-TagFailure { & $publishScript -Tag 'v2.0.15' -Commit $secondCommit -Remote $missingRemote } '*ls-remote*失败*' '远端不可访问时立即失败'
    Assert-TagEqual (@(Invoke-TestGit $workingRepository @('tag', '--list', 'v2.0.15')).Count) 0 '远端不可访问时不占用本地版本标签'

    # 通过本地 receive 配置模拟服务端拒绝写入，确保 git push 失败不会被视为发布成功。
    Invoke-TestGit $bareRepository @('config', 'receive.hideRefs', 'refs/tags/v2.0.16') | Out-Null
    Assert-TagFailure { & $publishScript -Tag 'v2.0.16' -Commit $secondCommit -Remote 'fixture' } '*push*失败*' '服务器拒绝标签推送时返回失败'
    Assert-TagEqual (@(Invoke-TestGit $bareRepository @('tag', '--list', 'v2.0.16')).Count) 0 '推送被拒绝后远端不产生标签'
    Invoke-TestGit $bareRepository @('config', '--unset', 'receive.hideRefs') | Out-Null
    & $publishScript -Tag 'v2.0.16' -Commit $secondCommit -Remote 'fixture'
    Assert-TagEqual ([string](Invoke-TestGit $bareRepository @('rev-parse', 'v2.0.16^{commit}'))) $secondCommit '远端恢复后可重试同一本地标签'
}
finally {
    if ($locationPushed) { Pop-Location }
    if (Test-Path -LiteralPath $fixtureRoot) {
        # 删除前核对本次夹具绝对路径，且只允许清理系统临时目录中的本次测试目录。
        $resolvedFixture = (Resolve-Path -LiteralPath $fixtureRoot).Path
        $expectedPrefix = Join-Path $temporaryBase 'xy-nuget-tag-tests-'
        if (-not $resolvedFixture.StartsWith($expectedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
            throw "拒绝清理临时目录之外的路径：$resolvedFixture"
        }
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force
    }
}

Write-Host "NuGet 标签回归测试通过：$script:PassedCount 项。"

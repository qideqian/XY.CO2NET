<#
.SYNOPSIS
把版本标签绑定到指定提交并推送；同一提交重跑可复用标签，绝不移动已有标签。
.PARAMETER Tag
待发布的 v主版本.次版本.补丁版本 标签。
.PARAMETER Commit
本次工作流实际构建的完整提交哈希，不能使用运行期间变化的 master 分支头。
.PARAMETER Remote
标签推送目标，默认 origin；测试时可使用本地临时裸仓库。
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$')]
    [string] $Tag,
    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string] $Commit,
    [string] $Remote = 'origin'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-CheckedGit {
    <# 捕获 Git 输出并检查退出码，网络故障或权限不足时禁止继续上传 NuGet。 #>
    param([Parameter(Mandatory)][string[]] $Arguments)
    $output = & git @Arguments
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments[0]) 失败，退出码：$LASTEXITCODE" }
    return $output
}

$headCommit = Invoke-CheckedGit @('rev-parse', 'HEAD')
if ($headCommit -ne $Commit) { throw '工作区 HEAD 与本次构建提交不一致，拒绝创建标签。' }

# 同时检查轻量标签和注释标签的解引用结果，已有标签只允许指向原提交。
$remoteRefs = @(Invoke-CheckedGit @('ls-remote', '--tags', $Remote, "refs/tags/$Tag", "refs/tags/$Tag^{}"))
if ($remoteRefs.Count -gt 0) {
    $peeledRef = @($remoteRefs | Where-Object { $_.EndsWith("refs/tags/$Tag^{}") })
    $target = if ($peeledRef.Count -gt 0) { $peeledRef[0] } else { $remoteRefs[0] }
    if (($target -split '\s+')[0] -ne $Commit) { throw "远程标签 $Tag 已指向其他提交，禁止覆盖。" }
    Write-Host "复用远程标签：$Tag -> $Commit"
    return
}

$localTags = @(Invoke-CheckedGit @('tag', '--list', $Tag))
if ($localTags.Count -gt 0) {
    $localCommit = Invoke-CheckedGit @('rev-parse', "$Tag^{commit}")
    if ($localCommit -ne $Commit) { throw "本地标签 $Tag 已指向其他提交，禁止覆盖。" }
}
else {
    Invoke-CheckedGit @('-c', 'user.name=github-actions[bot]', '-c', 'user.email=41898282+github-actions[bot]@users.noreply.github.com', 'tag', '-a', $Tag, $Commit, '-m', "NuGet release $Tag")
}

# 不使用 --force；出现并发抢占时让 Git 拒绝，保留已有发布记录。
Invoke-CheckedGit @('push', $Remote, "refs/tags/${Tag}:refs/tags/$Tag")
Write-Host "已创建发布标签：$Tag -> $Commit"

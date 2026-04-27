# XY.AspNetCore.Mvc.Validation

面向 `Microsoft.AspNetCore.Mvc` 的参数验证扩展类库，提供基于 `ControllerBase` 的链式 ModelState 校验能力。

## 功能

- 通过 `Validator(...)` 快速创建校验容器
- 支持空值、正则、Email、手机号、IP、URL、长度、字节长度、枚举、数值区间等常用校验
- 校验失败自动写入 `ModelState`
- 支持 `stopWhileFail` 失败即中断链式校验

## 使用示例

```csharp
var result = this.Validator(input.Email, "邮箱", "Email", false)
    ?.NotNullOrEmpty(true)
    ?.IsEmail(true)
    ?.MaxLength(100, true);

if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

## 目标框架

- net6.0
- net10.0

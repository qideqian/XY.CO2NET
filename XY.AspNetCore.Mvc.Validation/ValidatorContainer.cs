using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// 参数验证容器，封装当前验证对象与 ModelState。
/// </summary>
/// <typeparam name="T">待验证对象类型。</typeparam>
public class ValidatorContainer<T>
{
    /// <summary>
    /// 初始化 <see cref="ValidatorContainer{T}" />。
    /// </summary>
    /// <param name="controller">当前控制器实例。</param>
    /// <param name="validatorObject">待验证对象。</param>
    /// <param name="valueName">前端显示名称。</param>
    /// <param name="htmlName">提交字段名称。</param>
    public ValidatorContainer(ControllerBase controller, T validatorObject, string valueName, string htmlName)
    {
        Controller = controller;
        ValidatorObject = validatorObject;
        ValueName = valueName;
        HtmlName = htmlName;
        ModelState = controller.ModelState;
    }

    /// <summary>
    /// 待验证对象。
    /// </summary>
    public T ValidatorObject { get; set; }

    /// <summary>
    /// 前端显示名称。
    /// </summary>
    public string ValueName { get; set; }

    /// <summary>
    /// 是否已中止链式校验。
    /// </summary>
    public bool Stopped { get; set; }

    /// <summary>
    /// 当前对象是否有效。
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// ModelState 字段键。
    /// </summary>
    public string HtmlName { get; set; }

    /// <summary>
    /// 当前控制器的 ModelState。
    /// </summary>
    public ModelStateDictionary ModelState { get; set; }

    /// <summary>
    /// 当前控制器实例。
    /// </summary>
    public ControllerBase Controller { get; set; }

    /// <summary>
    /// 添加验证错误到 ModelState。
    /// </summary>
    /// <param name="errorMessage">错误信息。</param>
    public void AddError(string errorMessage)
    {
        ModelState.AddModelError(HtmlName, errorMessage);
        IsValid = false;
    }
}

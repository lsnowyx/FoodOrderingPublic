using AdminPanel.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Services;

public static class MvcErrorHelper
{
    public static void AddToModelState(ModelStateDictionary modelState, Exception exception)
    {
        modelState.AddModelError(string.Empty, GetDisplayMessage(exception));
    }

    public static void SetSuccessMessage(ITempDataDictionary tempData, string message)
    {
        tempData[TempDataKeys.SuccessMessage] = message;
    }

    public static void SetErrorMessage(ITempDataDictionary tempData, string message)
    {
        tempData[TempDataKeys.ErrorMessage] = message;
    }

    public static void SetFirstModelStateErrorMessage(
        ITempDataDictionary tempData,
        ModelStateDictionary modelState,
        string fallbackMessage)
    {
        tempData[TempDataKeys.ErrorMessage] = GetFirstModelStateErrorMessage(
            modelState,
            fallbackMessage);
    }

    public static bool IsFormBusinessException(Exception exception)
    {
        return exception is ValidationException
            or ArgumentException
            or InvalidOperationException;
    }

    public static string GetDisplayMessage(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => exception.Message,
            ValidationException => exception.Message,
            ArgumentException => exception.Message,
            InvalidOperationException => exception.Message,
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            _ => "An unexpected error occurred."
        };
    }

    private static string GetFirstModelStateErrorMessage(
        ModelStateDictionary modelState,
        string fallbackMessage)
    {
        return modelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
            ?? fallbackMessage;
    }
}

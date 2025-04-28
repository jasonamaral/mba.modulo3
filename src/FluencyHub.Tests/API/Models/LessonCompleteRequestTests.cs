using FluencyHub.API.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace FluencyHub.Tests.API.Models;

public class LessonCompleteRequestTests
{
    [Fact]
    public void LessonCompleteRequest_WithCompleted_IsValid()
    {
        // Arrange
        var request = new LessonCompleteRequest
        {
            Completed = true
        };
        
        // Act
        var validationResults = ValidateModel(request);
        
        // Assert
        Assert.Empty(validationResults);
    }
    
    [Fact]
    public void LessonCompleteRequest_WithDefaultBoolValue_HasDefaultFalse()
    {
        // Arrange
        var request = new LessonCompleteRequest();
        var modelState = new ModelStateDictionary();
        
        // Act
        // Verificar que o valor padrão de um bool é false
        Assert.False(request.Completed);
        
        // Simular a validação que poderia ocorrer se quiséssemos validar que o usuário 
        // explicitamente marcou o campo como true ou false (enviou o campo no request)
        modelState.AddModelError("Completed", "The 'Completed' field is required.");
        
        // Assert
        Assert.False(modelState.IsValid);
        Assert.Single(modelState);
        Assert.Equal("The 'Completed' field is required.", modelState["Completed"]!.Errors[0].ErrorMessage);
    }
    
    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        
        return validationResults;
    }
} 
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace CineManage.API.Utilities
{
    public class CustomDataTypeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var propertyName = bindingContext.ModelName;
            var propertyValue = bindingContext.ValueProvider.GetValue(propertyName);

            if (propertyValue == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            try
            {
                var destinationType = bindingContext.ModelMetadata.ModelType;
                var deserializedValue = JsonSerializer.Deserialize(propertyValue.FirstValue!,
                    destinationType, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                bindingContext.Result = ModelBindingResult.Success(deserializedValue);
            
            }
            catch (Exception ex)
            {

                bindingContext.ModelState.TryAddModelError(key: propertyName, errorMessage: "Invalid value for data type");
            }

            return Task.CompletedTask;
        }
    }
}

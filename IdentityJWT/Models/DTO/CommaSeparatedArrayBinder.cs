using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace PinPinServer.Models.DTO
{
    public class CommaSeparatedArrayBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                var value = valueProviderResult.FirstValue;
                if (value != null)
                {
                    var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var intValues = Array.ConvertAll(values, int.Parse);
                    bindingContext.Result = ModelBindingResult.Success(intValues);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(Array.Empty<int>());
                }
            }

            return Task.CompletedTask;
        }
    }
}
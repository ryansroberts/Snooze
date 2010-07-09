using System.ComponentModel;
using System.Web.Mvc;
using System;

namespace Snooze
{
    public class InputModelBinder : DefaultModelBinder
    {
        public InputModelBinder(IModelBinder defaultBinder)
        {
            _innerBinder = defaultBinder;
        }

        IModelBinder _innerBinder;

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {            
            if (typeof(IInput).IsAssignableFrom(propertyDescriptor.PropertyType))
            {
                BindInputProperty(controllerContext, bindingContext, propertyDescriptor);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        void BindInputProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var binder = this.Binders.GetBinder(propertyDescriptor.PropertyType);
            if (binder is InputModelBinder) binder = _innerBinder;

            var context = new ModelBindingContext
            {
                ModelName = propertyDescriptor.Name,
                ModelState = bindingContext.ModelState,
                ModelType = typeof(string),
                ValueProvider = bindingContext.ValueProvider
            };
            var value = (string)binder.BindModel(controllerContext, context);
            var input = (IInput)Activator.CreateInstance(propertyDescriptor.PropertyType);
            if (value != null)
            {
                input.RawValue = value;
            }
            SetProperty(controllerContext, bindingContext, propertyDescriptor, input);
        }
    }
}

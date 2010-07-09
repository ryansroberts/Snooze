using System.ComponentModel;
using System.Web.Mvc;

namespace Snooze
{
    /// <summary>
    /// A model binder that will bind the Parent property of a SubUrl.
    /// </summary>
    class SubUrlModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.Name == "Parent")
            {
                BindParentProperty(controllerContext, bindingContext, propertyDescriptor);
            }
            else
            {
                base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
            }
        }

        void BindParentProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            //var binder = ModelBinders.Binders.GetBinder(propertyDescriptor.PropertyType, true);
            var parentBindingContext = new ModelBindingContext
            {
                //ModelType = propertyDescriptor.PropertyType,  - not valid in .Net 4 replaced with following
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, propertyDescriptor.PropertyType),

                ValueProvider = bindingContext.ValueProvider
            };
            //var parent = binder.BindModel(controllerContext, parentBindingContext);
            var parent = BindModel(controllerContext, parentBindingContext);
            propertyDescriptor.SetValue(bindingContext.Model, parent);
        }
    }
}

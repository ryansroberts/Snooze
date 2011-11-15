using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using Spark;

namespace Snooze.MSpec
{
	public class DynamicViewDataDictionary : DynamicObject
	{
		private readonly Func<ViewDataDictionary> _viewDataThunk;

		public DynamicViewDataDictionary(ViewDataDictionary viewDataThunk)
		{
			_viewDataThunk = () => viewDataThunk;
		}

		private ViewDataDictionary ViewData
		{
			get
			{
				var viewData = _viewDataThunk();
				return viewData;
			}
		}

		// Implementing this function improves the debugging experience as it provides the debugger with the list of all
		// the properties currently defined on the object
		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return ViewData.Keys;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = ViewData[binder.Name];
			// since ViewDataDictionary always returns a result even if the key does not exist, always return true
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			ViewData[binder.Name] = value;
			// you can always set a key in the dictionary so return true 
			return true;
		}
	}

	public abstract class TestSparkView<TModel> : TestSparkView
	{
		private ViewDataDictionary<TModel> _viewData;

		public TModel Model
		{
			get { return ViewData.Model; }
		}

		public new ViewDataDictionary<TModel> ViewData
		{
			get
			{
				if (_viewData == null)
					SetViewData(new ViewDataDictionary<TModel>());
				return _viewData;
			}
			set { SetViewData(value); }
		}

		protected override void SetViewData(ViewDataDictionary viewData)
		{
			_viewData = new ViewDataDictionary<TModel>(viewData);
			base.SetViewData(_viewData);
		}
	}

	public abstract class TestSparkView : SparkViewBase
	{
		private ViewDataDictionary _viewData;
		private dynamic _viewBag;

		protected virtual void SetViewData(ViewDataDictionary viewData)
		{
			_viewData = viewData;
		}

		protected virtual void SetViewBag(DynamicViewDataDictionary viewBag)
		{
			_viewBag = viewBag;
		}

		public ViewDataDictionary ViewData
		{
			get
			{
				if (_viewData == null)
					SetViewData(new ViewDataDictionary());
				return _viewData;
			}
			set { SetViewData(value); }
		}

		public HtmlHelper Html { get; set; }

		public dynamic ViewBag
		{
			get
			{
				if (_viewBag == null)
					SetViewBag(new DynamicViewDataDictionary(ViewData));
				return _viewBag;
			}
		}

	
		public string H(object value)
		{
			if (value is MvcHtmlString)
				return H((MvcHtmlString)value);
			return HttpUtility.HtmlEncode(value);
		}

		public string H(MvcHtmlString value)
		{
			return value == null ? null : value.ToString();
		}

		public MvcHtmlString HTML(object value)
		{
			return MvcHtmlString.Create(Convert.ToString(value));
		}

		public object Eval(string expression)
		{
			return ViewData.Eval(expression);
		}
		public string Eval(string expression, string format)
		{
			return ViewData.Eval(expression, format);
		}
	}
}
using Castle.Windsor;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Registration;
using Snooze.Mspecc.MoqContrib.AutoMock;

namespace Snooze.Mspecc.AutoMock.Castle
{
    /// <summary>
    /// A WindsorContainer that auto-generates missing components as mocks
    /// </summary>
    public class AutoMockContainer : WindsorContainer, IAutoMockContainer
    {
        /// <summary>
        /// 
        /// </summary>
        internal protected IAutoMocker _helper;
		private ILatentMocker _latentMocker;

    	public IWindsorContainer Container
    	{
    		get { return _helper.Container; }
    	}

    	public  void UseContainer(IWindsorContainer container ) { _helper.UseContainer(container); }

        /// <summary>
        /// For unit testing. I can't think of why we need this for anything else
        /// </summary>
        /// <param name="helper"></param>
		/// <param name="latentMocker"></param>
        internal AutoMockContainer(IAutoMocker helper, ILatentMocker latentMocker)
        {
			_helper = helper;
			_latentMocker = latentMocker;
			Initialize();
		}

        private AutoMockContainer(AutoMockHelper helper)
        {
            _helper = helper;
			_latentMocker = helper;
            // FIXME: This feels like object incest. Is it ok?
            helper.Container = this;
			Initialize();
        }

		private void Initialize()
		{
			Register(Component.For<ILazyComponentLoader>().Instance(new LazyLoader(_latentMocker)));
		}

        /// <summary>
        /// Create an instance of the container
        /// </summary>
        public AutoMockContainer() : this(new AutoMockHelper()) { 
        }
        
        #region IAutoMockContainer Members

        /// <summary>
        /// 
        /// </summary>
        public IAutoMocker Mock
        {
            get { return _helper; }
			// only for unit tests
			internal set { _helper = value; }
        }

        #endregion
    }

	public class AutoMockContainer<TUnderTest> : AutoMockContainer where TUnderTest : class
	{
		protected TUnderTest classUnderTest;

		public T Inject<T>(T instance) { 
			_helper.RegisterInstance(typeof(T), instance);
			return instance;
		}


		public TUnderTest ClassUnderTest
		{
			get { return classUnderTest ?? (classUnderTest = _helper.CreateTestSubject<TUnderTest>()); }
		}
	}
}

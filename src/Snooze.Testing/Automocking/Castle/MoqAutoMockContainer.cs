using Castle.Core.Configuration;
using Castle.Facilities.LightweighFactory;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Registration;
using Moq;
using Snooze.AutoMock.Castle.MoqContrib.AutoMock;

namespace Snooze.AutoMock.Castle
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

		public static readonly string FactoryKey = "lightweight-factory";
		public static readonly string DelegateBuilderKey = "lightweight-factory-delegate-builder";

		
		private void Initialize()
		{
			Kernel.Resolver.AddSubResolver(new ParametersBinder());
            Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
			if (!Kernel.HasComponent(FactoryKey))
			{
				Kernel.AddComponent(FactoryKey, typeof(ILazyComponentLoader), typeof(LightweightFactory));
			}
			if (!Kernel.HasComponent(DelegateBuilderKey))
			{
				Kernel.AddComponent(DelegateBuilderKey, typeof(IDelegateBuilder),
									typeof(ExpressionTreeBasedDelegateBuilder));
			}
	
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

        public Mock<T> CreateMock<T>() where T : class
        {
            return _helper.Get<T>();
        }

        public T Inject<T>(T instance)
        {
            _helper.RegisterInstance(typeof(T), instance);
            return instance;
        }

        public void InjectArray<T>(T[] objects)
        {
            if (objects == null)
                return;
            _helper.RegisterInstances(typeof(T), objects.Cast<object>().ToArray());
        }

        #endregion
    }

	public class AutoMockContainer<TUnderTest> : AutoMockContainer where TUnderTest : class
	{
		protected TUnderTest classUnderTest;


		public TUnderTest ClassUnderTest
		{
			get { return classUnderTest ?? (classUnderTest = _helper.CreateTestSubject<TUnderTest>()); }
		}

	    
	}
}

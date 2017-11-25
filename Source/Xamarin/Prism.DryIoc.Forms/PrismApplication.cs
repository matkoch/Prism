﻿using DryIoc;
using Prism.Common;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using Xamarin.Forms;

namespace Prism.DryIoc
{
    /// <summary>
    /// Application base class using DryIoc
    /// </summary>
    public abstract class PrismApplication : PrismApplicationBase<IContainer>
    {
        protected PrismApplication(IPlatformInitializer platformInitializer = null)
            : base(platformInitializer) { }

        /// <summary>
        /// Creates the <see cref="IContainerExtension"/> for DryIoc
        /// </summary>
        /// <returns></returns>
        protected override IContainerExtension<IContainer> CreateContainerExtension()
        {
            return new DryIocContainerExtension(new Container(CreateContainerRules()));
        }

        /// <summary>
        /// Create <see cref="Rules" /> to alter behavior of <see cref="IContainer" />
        /// </summary>
        /// <returns>An instance of <see cref="Rules" /></returns>
        protected virtual Rules CreateContainerRules() => Rules.Default.WithAutoConcreteTypeResolution();

        /// <summary>
        /// Configures the Container.
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void ConfigureContainer(IContainerRegistry containerRegistry)
        {
            base.ConfigureContainer(containerRegistry);
            Container.Instance.Register<INavigationService, PageNavigationService>();
            Container.Instance.Register<INavigationService>(
                made: Made.Of(() => SetPage(Arg.Of<INavigationService>(), Arg.Of<Page>())),
                setup: Setup.Decorator);
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                switch (view)
                {
                    case Page page:
                        var getVM = Container.Instance.Resolve<Func<Page, object>>(type);
                        return getVM(page);
                    default:
                        return Container.Resolve(type);
                }
            });
        }

        internal static INavigationService SetPage(INavigationService navigationService, Page page)
        {
            if (navigationService is IPageAware pageAware)
            {
                pageAware.Page = page;
            }

            return navigationService;
        }
    }
}
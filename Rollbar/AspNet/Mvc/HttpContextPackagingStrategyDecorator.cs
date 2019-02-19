﻿#if NETFX

namespace Rollbar.AspNet.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using Rollbar.Common;
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;

    /// <summary>
    /// Class HttpContextPackagingStrategyDecorator.
    /// Implements the <see cref="Rollbar.RollbarPackagingStrategyDecoratorBase" /></summary>
    /// <seealso cref="Rollbar.RollbarPackagingStrategyDecoratorBase" />
    public class HttpContextPackagingStrategyDecorator
                : RollbarPackagingStrategyDecoratorBase
    {
        /// <summary>
        /// The strategy to decorate
        /// </summary>
        private readonly IRollbarPackagingStrategy _strategyToDecorate;
        /// <summary>
        /// The HTTP context
        /// </summary>
        private readonly HttpContext _httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextPackagingStrategyDecorator" /> class.
        /// </summary>
        /// <param name="strategyToDecorate">The strategy to decorate.</param>
        /// <param name="httpContext">The HTTP context.</param>
        public HttpContextPackagingStrategyDecorator(IRollbarPackagingStrategy strategyToDecorate, HttpContext httpContext)
                    : base(strategyToDecorate, false)
        {
            Assumption.AssertNotNull(httpContext, nameof(httpContext));

            this._httpContext = httpContext;

            this._strategyToDecorate = strategyToDecorate;
        }

        /// <summary>
        /// Decorates the specified rollbar data.
        /// </summary>
        /// <param name="rollbarData">The rollbar data.</param>
        protected override void Decorate(Data rollbarData)
        {
            if (this._httpContext?.Request != null)
            {
                // here we essentially piggy-back on capabilities of 
                // already implemented HttpRequestPackagingStrategyDecorator 
                // instead of this decorator:
                HttpRequestPackagingStrategyDecorator strategy =
                    new HttpRequestPackagingStrategyDecorator(this._strategyToDecorate, new HttpRequestWrapper(this._httpContext.Request));
                strategy.PackageAsRollbarData();
            }
        }
    }
}

#endif

using System;
using Microsoft.Extensions.Logging;

namespace Polly.Contrib.LoggingPolicy
{
    /// <summary>
    /// Contains configuration syntax for the <see cref="AsyncLoggingPolicy{TResult}"/>
    /// </summary>
    public static class AsyncLoggingPolicyTResultSyntax
    {
        /// <summary>
        /// Constructs a new instance of <see cref="AsyncLoggingPolicy{TResult}"/>, configured to handle the exceptions and results specified in the <paramref name="policyBuilder"/>.
        /// </summary>
        /// <typeparam name="TResult">The return type of delegates which may be executed through the policy.</typeparam>
        /// <param name="policyBuilder">The policy builder.</param>
        /// <param name="loggerProvider">A func returning a logger to use.</param>
        /// <param name="logAction">A logging action.</param>
        /// <returns><see cref="AsyncLoggingPolicy{TResult}"/></returns>
        public static AsyncLoggingPolicy<TResult> AsyncLog<TResult>(this PolicyBuilder<TResult> policyBuilder,
            Func<Context, ILogger> loggerProvider,
            Action<ILogger, Context, DelegateResult<TResult>> logAction
        )
        {
            return new AsyncLoggingPolicy<TResult>(
                policyBuilder,
                loggerProvider,
                logAction
            );
        }
    }
}

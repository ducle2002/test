namespace Castle.Services.Logging.Log4netIntegration
{
	using System;
	using System.IO;
	using System.Reflection;

	using Castle.Core.Logging;

	using log4net;
	using log4net.Config;

	public class Log4netFactory : AbstractLoggerFactory
	{
		internal const string defaultConfigFileName = "log4net.tps.config";
		static readonly Assembly _callingAssembly = typeof(Log4netFactory).Assembly;

		public Log4netFactory() : this(defaultConfigFileName)
		{
		}

		public Log4netFactory(string configFile)
		{
			var file = GetConfigFile(configFile);
			XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(_callingAssembly), file);
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="Log4netFactory" /> class.
		/// </summary>
		/// <param name="configuredExternally"> If <c>true</c> . Skips the initialization of log4net assuming it will happen externally. Useful if you're using another framework that wants to take over configuration of log4net. </param>
		public Log4netFactory(bool configuredExternally)
		{
			if (configuredExternally)
			{
				return;
			}

			var file = GetConfigFile(defaultConfigFileName);
			XmlConfigurator.ConfigureAndWatch(LogManager.GetRepository(_callingAssembly), file);
		}

		/// <summary>
		///   Configures log4net with a stream containing XML.
		/// </summary>
		public Log4netFactory(Stream config)
		{
			XmlConfigurator.Configure(LogManager.GetRepository(_callingAssembly), config);
		}

		public override ILogger Create(string name)
		{
			var log = LogManager.GetLogger(_callingAssembly, name);
			return new Log4netLogger(log, this);
		}

		public override ILogger Create(string name, LoggerLevel level)
		{
			throw new NotSupportedException("Logger levels cannot be set at runtime. Please review your configuration file.");
		}
	}
}



namespace Castle.Services.Logging.Log4netIntegration
{
	using System;
	using System.Globalization;

	using log4net;
	using log4net.Core;
	using log4net.Util;

#if FEATURE_SERIALIZATION
	[Serializable]
#endif
	public class Log4netLogger : Castle.Core.Logging.ILogger
	{
		private static readonly Type declaringType = typeof(Log4netLogger);

		public Log4netLogger(ILogger logger, Log4netFactory factory)
		{
			Logger = logger;
			Factory = factory;
		}

		internal Log4netLogger()
		{
		}

		internal Log4netLogger(ILog log, Log4netFactory factory) : this(log.Logger, factory)
		{
		}

		public bool IsTraceEnabled
		{
			get { return Logger.IsEnabledFor(Level.Trace); }
		}

		public bool IsDebugEnabled
		{
			get { return Logger.IsEnabledFor(Level.Debug); }
		}

		public bool IsErrorEnabled
		{
			get { return Logger.IsEnabledFor(Level.Error); }
		}

		public bool IsFatalEnabled
		{
			get { return Logger.IsEnabledFor(Level.Fatal); }
		}

		public bool IsInfoEnabled
		{
			get { return Logger.IsEnabledFor(Level.Info); }
		}

		public bool IsWarnEnabled
		{
			get { return Logger.IsEnabledFor(Level.Warn); }
		}

		protected internal Log4netFactory Factory { get; set; }

		protected internal ILogger Logger { get; set; }

		public override string ToString()
		{
			return Logger.ToString();
		}

		public virtual Castle.Core.Logging.ILogger CreateChildLogger(string name)
		{
			return Factory.Create(Logger.Name + "." + name);
		}

		public void Trace(string message)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, message, null);
			}
		}

		public void Trace(Func<string> messageFactory)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, messageFactory.Invoke(), null);
			}
		}

		public void Trace(string message, Exception exception)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, message, exception);
			}
		}

		public void TraceFormat(string format, params object[] args)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void TraceFormat(Exception exception, string format, params object[] args)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void TraceFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsTraceEnabled)
			{
				Logger.Log(declaringType, Level.Trace, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}

		public void Debug(string message)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, message, null);
			}
		}

		public void Debug(Func<string> messageFactory)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, messageFactory.Invoke(), null);
			}
		}

		public void Debug(string message, Exception exception)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, message, exception);
			}
		}

		public void DebugFormat(string format, params object[] args)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void DebugFormat(Exception exception, string format, params object[] args)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsDebugEnabled)
			{
				Logger.Log(declaringType, Level.Debug, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}

		public void Error(string message)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, message, null);
			}
		}

		public void Error(Func<string> messageFactory)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, messageFactory.Invoke(), null);
			}
		}

		public void Error(string message, Exception exception)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, message, exception);
			}
		}

		public void ErrorFormat(string format, params object[] args)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void ErrorFormat(Exception exception, string format, params object[] args)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsErrorEnabled)
			{
				Logger.Log(declaringType, Level.Error, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}

		public void Fatal(string message)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, message, null);
			}
		}

		public void Fatal(Func<string> messageFactory)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, messageFactory.Invoke(), null);
			}
		}

		public void Fatal(string message, Exception exception)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, message, exception);
			}
		}

		public void FatalFormat(string format, params object[] args)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void FatalFormat(Exception exception, string format, params object[] args)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsFatalEnabled)
			{
				Logger.Log(declaringType, Level.Fatal, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}

		public void Info(string message)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, message, null);
			}
		}

		public void Info(Func<string> messageFactory)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, messageFactory.Invoke(), null);
			}
		}

		public void Info(string message, Exception exception)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, message, exception);
			}
		}

		public void InfoFormat(string format, params object[] args)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void InfoFormat(Exception exception, string format, params object[] args)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsInfoEnabled)
			{
				Logger.Log(declaringType, Level.Info, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}

		public void Warn(string message)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, message, null);
			}
		}

		public void Warn(Func<string> messageFactory)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, messageFactory.Invoke(), null);
			}
		}

		public void Warn(string message, Exception exception)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, message, exception);
			}
		}

		public void WarnFormat(string format, params object[] args)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		public void WarnFormat(Exception exception, string format, params object[] args)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
			}
		}

		public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), null);
			}
		}

		public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
		{
			if (IsWarnEnabled)
			{
				Logger.Log(declaringType, Level.Warn, new SystemStringFormat(formatProvider, format, args), exception);
			}
		}
	}
}
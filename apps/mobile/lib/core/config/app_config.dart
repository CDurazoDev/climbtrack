class AppConfig {
  static const String apiBaseUrl = String.fromEnvironment('API_BASE_URL');
  static const String sentryDsn = String.fromEnvironment('SENTRY_DSN');
  static const String environment = String.fromEnvironment('ENV', defaultValue: 'dev');
  static const bool enableDebugLogging = bool.fromEnvironment('ENABLE_DEBUG_LOGGING', defaultValue: false);

  static bool get hasSentryDsn => sentryDsn.isNotEmpty;
}

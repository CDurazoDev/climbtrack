import 'package:flutter/material.dart';
import 'package:sentry_flutter/sentry_flutter.dart';

import 'core/config/app_config.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  if (!AppConfig.hasSentryDsn) {
    runApp(const MaterialApp());
    return;
  }

  await SentryFlutter.init(
    (options) {
      options.dsn = AppConfig.sentryDsn;
      options.environment = AppConfig.environment;
      options.debug = AppConfig.enableDebugLogging;
    },
    appRunner: () => runApp(const MaterialApp()),
  );
}

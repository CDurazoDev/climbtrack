import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:sentry_flutter/sentry_flutter.dart';

import 'app.dart';
import 'core/config/app_config.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  if (!AppConfig.hasSentryDsn) {
    runApp(const ProviderScope(child: ClimbTrackApp()));
    return;
  }

  await SentryFlutter.init(
    (options) {
      options.dsn = AppConfig.sentryDsn;
      options.environment = AppConfig.environment;
      options.debug = AppConfig.enableDebugLogging;
    },
    appRunner: () => runApp(const ProviderScope(child: ClimbTrackApp())),
  );
}

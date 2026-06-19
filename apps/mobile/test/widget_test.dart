import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:climbtrack/app.dart';

void main() {
  testWidgets('App smoke test', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    // Wrapped in ProviderScope because ClimbTrackApp uses Riverpod
    await tester.pumpWidget(
      const ProviderScope(
        child: ClimbTrackApp(),
      ),
    );

    // Verify that the registration page is shown (based on your RegisterPage content)
    expect(find.text('Crea tu cuenta'), findsOneWidget);

    // Verify that the "Crear cuenta" button is present
    expect(find.widgetWithText(ElevatedButton, 'Crear cuenta'), findsOneWidget);
  });
}

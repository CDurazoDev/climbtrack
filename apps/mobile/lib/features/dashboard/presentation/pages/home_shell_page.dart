import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/constants/layout_constants.dart';
import '../../../../core/theme/app_colors.dart';

class HomeShellPage extends StatelessWidget {
  const HomeShellPage({
    super.key,
    required this.location,
    required this.child,
  });

  final String location;
  final Widget child;

  int get _currentIndex => location.startsWith('/home/plan') ? 1 : 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: child,
      bottomNavigationBar: Container(
        height: kBottomNavH + MediaQuery.of(context).padding.bottom,
        padding: EdgeInsets.fromLTRB(
          kGapMd,
          10,
          kGapMd,
          10 + MediaQuery.of(context).padding.bottom,
        ),
        decoration: BoxDecoration(
          color: AppColors.surfaceVariant,
          border: Border(
              top: BorderSide(color: Colors.white.withValues(alpha: 0.06))),
        ),
        child: NavigationBar(
          selectedIndex: _currentIndex,
          backgroundColor: Colors.transparent,
          indicatorColor: AppColors.primary.withValues(alpha: 0.14),
          destinations: const [
            NavigationDestination(
              icon: Icon(Icons.home_outlined),
              selectedIcon: Icon(Icons.home_rounded),
              label: 'Dashboard',
            ),
            NavigationDestination(
              icon: Icon(Icons.timeline_outlined),
              selectedIcon: Icon(Icons.timeline_rounded),
              label: 'Plan',
            ),
          ],
          onDestinationSelected: (index) {
            context.go(index == 0 ? '/home/dashboard' : '/home/plan');
          },
        ),
      ),
    );
  }
}

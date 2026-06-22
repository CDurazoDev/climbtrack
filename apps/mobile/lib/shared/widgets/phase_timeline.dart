import 'package:flutter/material.dart';

import '../../core/constants/layout_constants.dart';
import '../../core/theme/app_colors.dart';
import '../../core/theme/app_typography.dart';

class PhaseTimelineSegment {
  const PhaseTimelineSegment({
    required this.label,
    required this.color,
    required this.startWeek,
    required this.endWeek,
  });

  final String label;
  final Color color;
  final int startWeek;
  final int endWeek;
}

class PhaseTimeline extends StatelessWidget {
  const PhaseTimeline({
    super.key,
    required this.segments,
    required this.currentWeek,
    required this.totalWeeks,
  });

  final List<PhaseTimelineSegment> segments;
  final int currentWeek;
  final int totalWeeks;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          height: 60,
          child: CustomPaint(
            size: const Size(double.infinity, 60),
            painter: _PhaseTimelinePainter(
              segments: segments,
              currentWeek: currentWeek,
              totalWeeks: totalWeeks,
            ),
          ),
        ),
        const SizedBox(height: kGapMd),
        Wrap(
          spacing: kGapMd,
          runSpacing: kGapSm,
          children: segments
              .map(
                (segment) => Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Container(
                      width: 10,
                      height: 10,
                      decoration: BoxDecoration(
                        color: segment.color,
                        borderRadius: BorderRadius.circular(999),
                      ),
                    ),
                    const SizedBox(width: 6),
                    Text(segment.label, style: AppTypography.bodySm),
                  ],
                ),
              )
              .toList(),
        ),
      ],
    );
  }
}

class _PhaseTimelinePainter extends CustomPainter {
  _PhaseTimelinePainter({
    required this.segments,
    required this.currentWeek,
    required this.totalWeeks,
  });

  final List<PhaseTimelineSegment> segments;
  final int currentWeek;
  final int totalWeeks;

  @override
  void paint(Canvas canvas, Size size) {
    final trackRect = RRect.fromRectAndRadius(
      Rect.fromLTWH(0, 10, size.width, 18),
      const Radius.circular(kRadiusMd),
    );

    final backgroundPaint = Paint()..color = AppColors.surfaceCard;
    canvas.drawRRect(trackRect, backgroundPaint);

    for (final segment in segments) {
      final startFraction = (segment.startWeek - 1) / totalWeeks;
      final endFraction = segment.endWeek / totalWeeks;
      final rect = RRect.fromRectAndRadius(
        Rect.fromLTWH(
          size.width * startFraction,
          10,
          size.width * (endFraction - startFraction),
          18,
        ),
        const Radius.circular(kRadiusMd),
      );

      canvas.drawRRect(rect, Paint()..color = segment.color);
    }

    final weekMarkerX = size.width * (((currentWeek - 1) + 0.5) / totalWeeks);
    final markerPaint = Paint()
      ..color = AppColors.primary
      ..strokeWidth = 2;

    canvas.drawLine(
        Offset(weekMarkerX, 4), Offset(weekMarkerX, 34), markerPaint);

    final trianglePath = Path()
      ..moveTo(weekMarkerX - 6, 40)
      ..lineTo(weekMarkerX + 6, 40)
      ..lineTo(weekMarkerX, 48)
      ..close();
    canvas.drawPath(trianglePath, Paint()..color = AppColors.primary);
  }

  @override
  bool shouldRepaint(covariant _PhaseTimelinePainter oldDelegate) {
    return oldDelegate.segments != segments ||
        oldDelegate.currentWeek != currentWeek ||
        oldDelegate.totalWeeks != totalWeeks;
  }
}

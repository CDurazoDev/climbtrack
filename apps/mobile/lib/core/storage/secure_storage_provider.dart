import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

import '../../features/auth/data/models/auth_tokens_model.dart';

const _accessTokenKey = 'access_token';
const _refreshTokenKey = 'refresh_token';
const _userProfileKey = 'user_profile';

final secureStorageProvider = Provider<FlutterSecureStorage>((ref) {
  return const FlutterSecureStorage();
});

extension SecureStorageAuthX on FlutterSecureStorage {
  Future<void> saveAuthTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    await write(key: _accessTokenKey, value: accessToken);
    await write(key: _refreshTokenKey, value: refreshToken);
  }

  Future<void> saveUserProfile(UserProfileModel user) async {
    await write(key: _userProfileKey, value: jsonEncode(user.toJson()));
  }

  Future<String?> readAccessToken() => read(key: _accessTokenKey);

  Future<String?> readRefreshToken() => read(key: _refreshTokenKey);

  Future<UserProfileModel?> readUserProfile() async {
    final jsonValue = await read(key: _userProfileKey);
    if (jsonValue == null || jsonValue.isEmpty) {
      return null;
    }

    return UserProfileModel.fromJson(jsonDecode(jsonValue) as Map<String, dynamic>);
  }

  Future<void> clearAuthSession() async {
    await delete(key: _accessTokenKey);
    await delete(key: _refreshTokenKey);
    await delete(key: _userProfileKey);
  }
}

/*
 * Copyright (C) 2015 Square, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package okhttp3.java.net.cookiejar;

import java.io.IOException;
import java.net.CookieHandler;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;
import okhttp3.Cookie;
import okhttp3.CookieJar;
import okhttp3.HttpUrl;

/** A cookie jar that delegates to a {@link java.net.CookieHandler}. */
public class JavaNetCookieJar implements CookieJar {

    private final CookieHandler cookieHandler;

    public JavaNetCookieJar(CookieHandler cookieHandler) {
        this.cookieHandler = cookieHandler;
    }

    @Override
    public void saveFromResponse(HttpUrl url, List<Cookie> cookies) {
        List<String> cookieStrings = new ArrayList<>();
        for (Cookie cookie : cookies) {
            cookieStrings.add(cookieToString(cookie, true));
        }
        Map<String, List<String>> multimap = Collections.singletonMap("Set-Cookie", cookieStrings);
        try {
            cookieHandler.put(url.uri(), multimap);
        } catch (IOException ignored) {
        }
    }

    @Override
    public List<Cookie> loadForRequest(HttpUrl url) {
        Map<String, List<String>> cookieHeaders;
        try {
            cookieHeaders = cookieHandler.get(url.uri(), Collections.<String, List<String>>emptyMap());
        } catch (IOException ignored) {
            return Collections.emptyList();
        }

        List<Cookie> cookies = null;
        for (Map.Entry<String, List<String>> entry : cookieHeaders.entrySet()) {
            String key = entry.getKey();
            List<String> values = entry.getValue();
            if (("Cookie".equalsIgnoreCase(key) || "Cookie2".equalsIgnoreCase(key)) && !values.isEmpty()) {
                for (String header : values) {
                    if (cookies == null) cookies = new ArrayList<>();
                    cookies.addAll(decodeHeaderAsJavaNetCookies(url, header));
                }
            }
        }

        return cookies != null ? Collections.unmodifiableList(cookies) : Collections.<Cookie>emptyList();
    }

    /**
     * Convert a request header to OkHttp's cookies via {@link java.net.HttpCookie}. That extra step
     * handles multiple cookies in a single request header, which {@link Cookie#parse} doesn't support.
     */
    private List<Cookie> decodeHeaderAsJavaNetCookies(HttpUrl url, String header) {
        List<Cookie> result = new ArrayList<>();
        int pos = 0;
        int limit = header.length();
        while (pos < limit) {
            int pairEnd = delimiterOffset(header, ";,", pos, limit);
            int equalsSign = delimiterOffset(header, "=", pos, pairEnd);
            String name = trimSubstring(header, pos, equalsSign);
            if (name.startsWith("$")) {
                pos = pairEnd + 1;
                continue;
            }

            String value = equalsSign < pairEnd ? trimSubstring(header, equalsSign + 1, pairEnd) : "";
            if (value.startsWith("\"") && value.endsWith("\"") && value.length() >= 2) {
                value = value.substring(1, value.length() - 1);
            }

            result.add(new Cookie.Builder()
                    .name(name)
                    .value(value)
                    .domain(url.host())
                    .build());
            pos = pairEnd + 1;
        }
        return result;
    }

    // https://github.com/square/okhttp/blob/master/okhttp/src/commonJvmAndroid/kotlin/okhttp3/Cookie.kt
    private String cookieToString(Cookie cookie, boolean forObsoleteRfc2965) {
        StringBuilder sb = new StringBuilder();
        sb.append(cookie.name());
        sb.append('=');
        sb.append(cookie.value());

        if (cookie.persistent()) {
            if (cookie.expiresAt() == Long.MIN_VALUE) {
                sb.append("; max-age=0");
            } else {
                sb.append("; expires=").append(toHttpDateString(new Date(cookie.expiresAt())));
            }
        }

        if (!cookie.hostOnly()) {
            sb.append("; domain=");
            if (forObsoleteRfc2965) {
                sb.append('.');
            }
            sb.append(cookie.domain());
        }

        sb.append("; path=").append(cookie.path());

        if (cookie.secure()) {
            sb.append("; secure");
        }

        if (cookie.httpOnly()) {
            sb.append("; httponly");
        }

        if (cookie.sameSite() != null) {
            sb.append("; samesite=").append(cookie.sameSite());
        }

        return sb.toString();
    }

    private static int delimiterOffset(String s, String delimiters, int pos, int limit) {
        for (int i = pos; i < limit; i++) {
            if (delimiters.indexOf(s.charAt(i)) >= 0) return i;
        }
        return limit;
    }

    private static String trimSubstring(String s, int start, int end) {
        int from = start;
        int to = end;
        while (from < to && s.charAt(from) <= ' ') from++;
        while (to > from && s.charAt(to - 1) <= ' ') to--;
        return s.substring(from, to);
    }

    private static String toHttpDateString(Date date) {
        SimpleDateFormat sdf = new SimpleDateFormat("EEE, dd MMM yyyy HH:mm:ss 'GMT'", Locale.US);
        sdf.setTimeZone(TimeZone.getTimeZone("GMT"));
        return sdf.format(date);
    }
}

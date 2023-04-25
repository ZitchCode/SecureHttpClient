package securehttpclient.okhttp;

import java.io.IOException;

import okhttp3.Headers;
import okhttp3.Interceptor;
import okhttp3.Request;
import okhttp3.Response;

public class HeadersOrderInterceptor implements Interceptor {
    
    private static final String HEADERS_ORDER_HEADER = "securehttpclient-headers-order";

    @Override
    public Response intercept(Chain chain) throws IOException {
        Request original = chain.request();

        if (original.header(HEADERS_ORDER_HEADER) == null) {
            return chain.proceed(original);
        }

        Request.Builder requestBuilder = original.newBuilder();

        String headersOrderHeader = original.header(HEADERS_ORDER_HEADER);
        String[] headersOrderArray = headersOrderHeader.split(";");

        for (String name : headersOrderArray) {
            if (original.header(name) != null) {
                String header = original.header(name);
                requestBuilder
                    .removeHeader(name)
                    .addHeader(name, header);
            }
        }

        Request request = requestBuilder
            .removeHeader(HEADERS_ORDER_HEADER)
            .build();

        return chain.proceed(request);
    }
}
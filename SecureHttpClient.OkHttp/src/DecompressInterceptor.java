package securehttpclient.okhttp;

import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.util.Locale;
import java.util.zip.Inflater;
import java.util.zip.InflaterInputStream;
import org.brotli.dec.BrotliInputStream;

import okhttp3.Headers;
import okhttp3.Interceptor;
import okhttp3.Response;
import okhttp3.ResponseBody;
import okio.BufferedSource;
import okio.GzipSource;
import okio.Okio;

public class DecompressInterceptor implements Interceptor {
    @Override
    public Response intercept(Chain chain) throws IOException {
        Response response = chain.proceed(chain.request());

        if (isCompressed(response)) {
            return decompress(response);
        } else {
            return response;
        }
    }

    private Response decompress(final Response response) throws IOException {

        ResponseBody body = response.body();
        if (body == null) {
            return response;
        }

        BufferedSource source = body.source();
        String encoding = response.header("Content-Encoding").toLowerCase(Locale.ROOT);

        BufferedSource decompressed;
        switch (encoding) {
            case "gzip": {
                // okio can decompress gzip natively, without bridging to an InputStream
                decompressed = Okio.buffer(new GzipSource(source));
                break;
            }
            case "deflate": {
                // Sniff the first two bytes without consuming them: a zlib stream (RFC 1950)
                // starts with 0x78 and a second byte <= 0xDA, anything else is raw deflate (RFC 1951).
                boolean zlibHeader = false;
                BufferedSource peek = source.peek();
                try {
                    if (peek.request(2)) {
                        byte[] headerBytes = peek.readByteArray(2);
                        zlibHeader = (headerBytes[0] & 0xFF) == 0x78 && (headerBytes[1] & 0xFF) <= 0xDA;
                    }
                } catch (EOFException e) {
                }
                // Inflater(nowrap): true means raw deflate (RFC 1951), false means zlib-wrapped (RFC 1950).
                InputStream inflaterStream = new InflaterInputStream(source.inputStream(), new Inflater(!zlibHeader));
                decompressed = Okio.buffer(Okio.source(inflaterStream));
                break;
            }
            case "br": {
                decompressed = Okio.buffer(Okio.source(new BrotliInputStream(source.inputStream())));
                break;
            }
            default: {
                return response;
            }
        }

        Headers strippedHeaders = response.headers().newBuilder()
                .removeAll("Content-Encoding")
                .removeAll("Content-Length")
                .build();

        // Stream the decompressed bytes instead of buffering the whole body in memory.
        // The decompressed length is unknown (-1), so consumers read until EOF.
        return response.newBuilder()
                .headers(strippedHeaders)
                .body(ResponseBody.create(body.contentType(), -1L, decompressed))
                .message(response.message())
                .build();
    }

    private boolean isCompressed(Response response) {
        String contentEncoding = response.header("Content-Encoding");
        return contentEncoding != null 
            && (contentEncoding.equalsIgnoreCase("gzip") || contentEncoding.equalsIgnoreCase("deflate") || contentEncoding.equalsIgnoreCase("br"));
    }
}

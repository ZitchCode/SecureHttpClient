package securehttpclient.okhttp;

import java.io.IOException;
import java.util.zip.Inflater;

import okhttp3.Headers;
import okhttp3.Interceptor;
import okhttp3.Request;
import okhttp3.Response;
import okhttp3.ResponseBody;
import okio.GzipSource;
import okio.InflaterSource;
import okio.Source;
import okio.Okio;

import org.brotli.dec.BrotliInputStream;

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

        if (response.body() == null) {
            return response;
        }

        Source source = null;
		switch(response.header("Content-Encoding").toLowerCase()) {
            case "gzip": {
				source = new GzipSource(response.body().source());
				break;
			}
            case "deflate": {
				source = new InflaterSource(response.body().source(), new Inflater());
				break;
			}
            case "br": {
				source = Okio.source(new BrotliInputStream(response.body().source().inputStream()));
				break;
			}
        }

        String bodyString = Okio.buffer(source).readUtf8();

        ResponseBody responseBody = ResponseBody.create(bodyString, response.body().contentType());

        Headers strippedHeaders = response.headers().newBuilder()
                .removeAll("Content-Encoding")
                .removeAll("Content-Length")
                .build();
        return response.newBuilder()
                .headers(strippedHeaders)
                .body(responseBody)
                .message(response.message())
                .build();
    }

    private Boolean isCompressed(Response response) {
        return response.header("Content-Encoding") != null
            && (response.header("Content-Encoding").toLowerCase().equals("gzip") 
                || response.header("Content-Encoding").toLowerCase().equals("deflate") 
                || response.header("Content-Encoding").toLowerCase().equals("br"));
    }
}
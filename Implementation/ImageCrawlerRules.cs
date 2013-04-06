using System;
using System.Collections.Concurrent;
using System.Linq;

using Crawler.Interfaces;
using HtmlAgilityPack;

namespace Crawler.Implementation
{
	public class ImageCrawlerRules : ICrawlerRules<HtmlDocument>
	{
		// This "bag" adds as a simple collection to keep track of which urls we have
		// already crawled. As the name of the class implies, this is thread-safe.
		private readonly ConcurrentBag<string> _crawled = new ConcurrentBag<string>();

		// The scope defines the url that must be the beginning of a valid url.
		private readonly string _scope;

		public ImageCrawlerRules(string scope)
		{
			_scope = scope;
		}

		public bool IsValidUri(Uri uri)
		{
			// Simple validation of the passed object.
			if (uri == null)
				return false;

			// If the uri contains a fragment, it is not valid.
			// We could also just check for #.
			if (!String.IsNullOrWhiteSpace(uri.Fragment))
				return false;

			// Since we do not wish to keep crawling the
			// same pages over and over, we keep track of
			// all pages that have been added to the crawler.
			if (_crawled.Contains(uri.AbsoluteUri))
				return false;

			// Since this is a simple crawler, we do not wish
			// to crawl any url that does not start with the given
			// scope. An example of a scope could be https://example.com/,
			// which that only allow beginning with that string.
			if (!uri.AbsoluteUri.StartsWith(_scope, StringComparison.OrdinalIgnoreCase))
				return false;

			// If we reach this point, the url is validated,
			// and has not been crawled before. Therefore we
			// add it to the collection of crawled urls.
			_crawled.Add(uri.AbsoluteUri);

			// The url is valid at this point.
			return true;
		}

		public bool IsValidPage(HtmlDocument document)
		{
			// Here we could for example allow only pages that
			// contains specific words.
			return true;
		}
	}
}
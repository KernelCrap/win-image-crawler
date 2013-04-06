using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Crawler.Interfaces;
using HtmlAgilityPack;
using NLog;

namespace Crawler.Implementation
{
	public class ImageCrawler : ICrawler, IDisposable
	{
		// Thanks to the magic of the .NET framework, we have an easy way to handle
		// the Producer/Consumer problem. This collection allows us to add urls to be
		// Consumed by the crawler, which will block until urls are available.
		private readonly BlockingCollection<Uri> _urls = new BlockingCollection<Uri>();

		// The constructor requires rules to be defined, the rules are used
		// by the crawler to validate urls and pages, generics are used
		// so that the interface could be used for other document types.
		private readonly ICrawlerRules<HtmlDocument> _rules;

		// The constructor also requires a processer to be defined, which
		// is called by the crawler once a page has been validated and downloaded.
		private readonly ICrawlerProcesser<HtmlDocument> _processer;

		// We use the HtmlAgilityPack to crawl the web :)
		private readonly HtmlWeb _web = new HtmlWeb();

		// For logging purposes we use NLog.
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public ImageCrawler(ICrawlerRules<HtmlDocument> rules, ICrawlerProcesser<HtmlDocument> processer, int threads)
		{
			// Assign the rules to be used by the crawler
			// to validate urls and pages for crawling.
			_rules = rules;

			// Assign the processer the crawler will call to
			// have valid pages (specified by the rules) processed.
			_processer = processer;

			// At this point we start a thread for crawling.
			// The thread will block until urls are added to
			// the BlockingCollection.
			Task.Factory.StartNew(() => Crawl(threads), TaskCreationOptions.LongRunning);
		}

		public void Start(Uri uri)
		{
			// To start crawling, all we need to do is
			// add the url to the BlockingCollection.
			Add(uri);
		}

		private void Add(Uri uri)
		{
			// Use the rules given to the crawler to validate
			// the given urls.
			if (!_rules.IsValidUri(uri))
				return;

			// Now that the url is valid, we add it
			// to the crawler.
			_urls.Add(uri);

			// Print the url to the log.
			Log.Debug("[Thread #{0}]: Add: {1}", Thread.CurrentThread.ManagedThreadId, uri.AbsoluteUri);
		}

		private void Crawl(int threads)
		{
			// Create a partitioner without buffering, which provides
			// support for low latency (items will be processed as soon
			// as they are available from the source).
			var src = Partitioner.Create(_urls.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);

			// Call the Crawl method for each url that has been added
			// to our BlockingCollection using a number of threads.
			// This will block the threads until items are available.
			Parallel.ForEach(src, new ParallelOptions { MaxDegreeOfParallelism = threads }, Crawl);
		}

		private void Crawl(Uri uri)
		{
			// Print the url to the log.
			Log.Debug("[Thread #{0}]: Crawling: {1}", Thread.CurrentThread.ManagedThreadId, uri.AbsoluteUri);

			// We then use the HtmlAgilityPack to download and parse the uri.
			// The call is wrapped by try-catch, since alot can go wrong when
			// downloading and parsing stuff from the internet.
			HtmlDocument doc = null;
			try
			{
				doc = _web.Load(uri.AbsoluteUri);
			}
			catch (ArgumentException ex)
			{
				Log.ErrorException(ex.Message, ex);
			}
			catch (WebException ex)
			{
				Log.ErrorException(ex.Message, ex);
			}

			// Make sure the doc is not null.
			if (doc == null)
				return;

			// So now we have a document, we validate it using the rules
			// defined for this crawler. And as above, if it fails, we
			// just return and don't go any further.
			if (!_rules.IsValidPage(doc))
				return;

			// So now we have a valid page, that means that it is ready
			// to be passed to the processer.
			_processer.Process(uri, doc);

			// Now that the page has been validated and processed, we are
			// ready to continue crawling, and to do that we need to get all
			// links on the page using XPath.
			var links = doc.DocumentNode.SelectNodes("//a[@href]");

			// If there are no links on the page, just return.
			if (links == null)
				return;

			// We want to crawl all links on the page, so we call the Add
			// method which validates them and adds them to the crawler.
			foreach (var link in links)
			{
				try
				{
					// Get the absolute path of the href link, due to our XPath
					// expression, we are sure that a href attribute is present.
					var path = new Uri(uri, link.Attributes["href"].Value);

					// We then add the path to be crawled.
					Add(path);
				}
				catch (UriFormatException ex)
				{
					Log.ErrorException(ex.Message, ex);
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				_urls.Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
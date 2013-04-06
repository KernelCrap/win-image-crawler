using System;

namespace Crawler.Interfaces
{
	public interface ICrawlerProcesser<in T>
	{
		void Process(Uri uri, T document);
	}
}
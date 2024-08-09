YouTube Data Scraper using Scrape.do

This project demonstrates how to scrape YouTube search results using the Scrape.do API, implemented in C#. The script automates the process of fetching video titles and URLs based on specific search queries and saves the data in a CSV file. It effectively handles pagination by utilizing continuation tokens provided by YouTube.

Features

-Automated YouTube Scraping: Extracts video titles and URLs from YouTube search results.

-Handles Pagination: Uses continuation tokens to fetch multiple pages of results.

-CSV Output: Saves the scraped data into a structured CSV file for easy analysis.

-Parallel Processing: Implements asynchronous tasks to optimize performance and reduce execution time.

Requirements

- .NET Core SDK (version 3.1 or higher)
- Scrape.do API Token (Sign up to obtain your API token)


Installation
1. Clone the repository:
```
git clone https://github.com/muratonurm/youtube-scrape.git
```

2. Navigate to the project directory:
```
cd yourprojectname
```

3. Restore the required packages:
```
dotnet restore
```

Usage
1. Obtain your Scrape.do API token and replace the placeholder in the code with your actual token.
2. Modify search parameters: Update the searchParams array in the Main method with your desired YouTube search queries.
3. Run the application:
```
dotnet run
```
4. Output: The scraped data will be saved in a CSV file named youtube_results.csv in the project directory.

Code Overview

- FetchYoutubeData: Sends HTTP requests to Scrape.do API to fetch YouTube search results. Handles both initial and subsequent requests using continuation tokens.
- ParseInitialData: Extracts initial JSON data from the YouTube search page HTML.
- ParseVideoData: Extracts video titles and URLs from the JSON data.
- WriteResultsToCsv: Saves the extracted data into a CSV file.

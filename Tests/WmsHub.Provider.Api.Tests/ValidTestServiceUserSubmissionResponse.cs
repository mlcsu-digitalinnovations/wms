namespace WmsHub.ProviderApi.Tests
{
  static class ValidServiceUserSubmissionRequest
  {
    public const string SubmissionStarted1 = 
      "[{ \"ubrn\": \"777666555444\", " +
      "\"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"started\"}]";

    public const string SubmissionStartedWithUpdates1 = "[{" +
      "\"ubrn\": \"777666555443\"," +
      "\"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"Started\"," +
      "\"updates\": [" +
      "    {" +
      "       \"date\": \"2020-01-17T00:00:00+00:00\"," +
      "       \"weight\": 120	}] }]";

    public const string SubmissionRejected1 = 
      "[{ \"ubrn\": \"777666555442\"," +
      " \"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"rejected\", " +
      "\"reason\": \"Reason why the service user was rejected\" }]";

    public const string SubmissionRejected2 = 
      "[{ \"ubrn\": \"777666555441\"," +
      " \"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"rejected\"," +
      "  \"reason\": \"Reason why the service user was rejected\" }]";

    public const string SubmissionComplete1 = 
      "[{ \"ubrn\": \"777666555440\", " +
      " \"date\": \"2020-01-18T00:00:00+00:00\",  \"type\": \"completed\" }]";

    public const string SubmissionCompletedWithUpdates = "[{" +
      "\"ubrn\": \"777666555439\"," +
      "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
      "  \"type\": \"Completed\"," +
      "  \"updates\": [" +
      "      {" +
      "          \"date\": \"2020-01-17T00:00:00+00:00\"," +
      "          \"coaching\": 20," +
      "          \"measure\": 9," +
      "          \"weight\": 95 }]}]";

    public const string SumbmissionTerminated = "[{" +
      "  \"ubrn\": \"777666555438\"," +
      "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
      "  \"type\": \"Terminated\"," +
      "  \"reason\": \"Reason why the service user was terminated " +
      "during programme\"	}]";

    public const string SubmissionUpdate = "[{" +
      "\"ubrn\": \"777666555437\"," +
      "\"type\": \"update\"," +
      "\"updates\": [{" +
      "    \"date\": \"2020-01-11T00:00:00+00:00\"," +
      "    \"measure\": 10" +
      "},{" +
      "    \"date\": \"2020-01-12T00:00:00+00:00\"," +
      "    \"weight\": 105" +
      "},{" +
    "			\"date\": \"2020-01-13T00:00:00+00:00\"," +
     "     \"coaching\": 50" +
      "},{" +
      "		\"date\": \"2020-01-15T00:00:00+00:00\"," +
      "    \"coaching\": 50," +
      "    \"measure\": 10," +
      "    \"weight\": 105 }]}]";

  }
}

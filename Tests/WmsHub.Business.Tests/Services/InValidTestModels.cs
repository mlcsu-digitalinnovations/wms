namespace WmsHub.Business.Tests.Services
{
  static class InvalidTestModels
  {
    public const string SubmissionStartedMissinguUbrn = "[{ \"ubrn\": \"\", " +
      "\"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"started\"}]";

    public const string SubmissionStartedUbrnTooSmall = 
      "[{ \"ubrn\": \"00002\", \"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"started\"}]";

    public const string SubmissionStartedUbrnNotNumeric =
    "[{ \"ubrn\": \"ABCDEF123456\", \"date\": \"2020-01-18T00:00:00+00:00\"," +
    " \"type\": \"started\"}]";

    public const string SubmissionStartedUbrnSelfReferral =
      "[{ \"ubrn\": \"SR0000000001\", \"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"started\"}]";

    public const string SubmissionStartedUbrnPharmacy =
      "[{ \"ubrn\": \"PR0000000001\", \"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"started\"}]";

    public const string SubmissionStartedMissinguDate =
      "[{ \"ubrn\": \"777666555449\", " +
      " \"type\": \"started\"}]";

    public const string SubmissionStartedMissinguType = 
      "[{ \"ubrn\": \"777666555448\", " +
      "\"date\": \"2020-01-18T00:00:00+00:00\"}]";

    public const string SubmissionStartedWithUpdatesMissingDate = "[{" +
      "\"ubrn\": \"777666555447\"," +
      "\"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"Started\"," +
      "\"updates\": [" +
      "    {\"weight\": 120	}] }]";

    public const string SubmissionStartDateBeforeReferralDate = "[{" +
      "\"ubrn\": \"777666555442\"," +
      "\"date\": \"2020-01-18T00:00:00+00:00\"," +
      " \"type\": \"Started\"," +
      "\"updates\": [{" +
      "     \"date\": \"2020-01-17T00:00:00+00:00\"," +
      "    \"weight\": 120	}] }]";

    public const string SubmissionUpdatesDateBeforeReferralDate = "[{" +
      "\"ubrn\": \"777666555442\"," +
      "\"date\": \"2020-02-18T00:00:00+00:00\"," +
      " \"type\": \"Started\"," +
      "\"updates\": [{" +
      "     \"date\": \"2020-01-17T00:00:00+00:00\"," +
      "    \"weight\": 120	}] }]";

    public const string SubmissionStartedWithUpdatesMissingWeight = "[{" +
        "\"ubrn\": \"777666555445\"," +
        "\"date\": \"2020-01-18T00:00:00+00:00\"," +
        " \"type\": \"Started\"," +
        "\"updates\": [" +
        "    {" +
        "       \"date\": \"2020-01-17T00:00:00+00:00\"	}] }]";

    public const string SubmissionRejectedMissingUbrm = 
      "[{\"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"rejected\", " +
      "\"reason\": \"Reason why the service user was rejected\" }]";

    public const string SubmissionRejectedMissingDate = 
      "[{ \"ubrn\": \"777666555444\"," +
  "  \"type\": \"rejected\", " +
  "\"reason\": \"Reason why the service user was rejected\" }]";

    public const string SubmissionRejectedMissingReason = 
      "[{ \"ubrn\": \"777666555443\"," +
      " \"date\": \"2020-01-18T00:00:00+00:00\", \"type\": \"rejected\" }]";

    public const string SubmissionComplete1 = 
      "[{ \"ubrn\": \"777666555442\", " +
      " \"date\": \"2020-01-18T00:00:00+00:00\",  \"type\": \"completed\" }]";
    public const string SubmissionCompleteMissingUbrm =
    "[{\"date\": \"2020-01-18T00:00:00+00:00\",  \"type\": \"completed\" }]";

    public const string SubmissionCompletedWithUpdates = "[{" +
      "\"ubrn\": \"777666555441\"," +
      "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
      "  \"type\": \"Completed\"," +
      "  \"updates\": [" +
      "      {" +
      "          \"date\": \"2020-01-17T00:00:00+00:00\"," +
      "          \"coaching\": 20," +
      "          \"measure\": 9," +
      "          \"weight\": 95.25 }]}]";


    public const string SubmissionCompletedWithUpdatesMissingValues = "[{" +
      "\"ubrn\": \"777666555440\"," +
      "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
      "  \"type\": \"Completed\"," +
      "  \"updates\": [{ \"date\": \"2020-01-17T00:00:00+00:00\" }]}]";

    public const string SubmissionTerminated = "[{" +
      "  \"ubrn\": \"777666555439\"," +
      "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
      "  \"type\": \"Terminated\"," +
      "  \"reason\": \"Reason why the service user was terminated " +
      "during programme\"	}]";

    public const string SubmissionUpdate = "[{" +
      "\"ubrn\": \"777666555438\"," +
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

    public const string SubmissionUpdateLocked = "[{" +
     "\"ubrn\": \"777666555437\"," +
     "\"type\": \"update\"," +
     "\"updates\": [{\"date\": \"2020-01-15T00:00:00+00:00\"," +
     "    \"coaching\": 50," +
     "    \"measure\": 10," +
     "    \"weight\": 105 }]}]";

    public const string SubmissionCompletedWithUpdatesWeightDecimal3 = "[{" +
  "\"ubrn\": \"777666555436\"," +
  "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
  "  \"type\": \"Completed\"," +
  "  \"updates\": [" +
  "      {" +
  "          \"date\": \"2020-01-17T00:00:00+00:00\"," +
  "          \"coaching\": 20," +
  "          \"measure\": 9," +
  "          \"weight\": 95.123 }]}]";

    public const string SubmissionCompletedWithUpdatesMaxCoaching = "[{" +
"\"ubrn\": \"777666555435\"," +
"  \"date\": \"2020-01-18T00:00:00+00:00\"," +
"  \"type\": \"Completed\"," +
"  \"updates\": [" +
"      {" +
"          \"date\": \"2020-01-17T00:00:00+00:00\"," +
"          \"coaching\": 101," +
"          \"measure\": 9," +
"          \"weight\": 95.12 }]}]";

    public const string SubmissionUpdateMaxWeight =
      "[{" +
      "\"ubrn\": \"777666555437\"," +
      "\"type\": \"update\"," +
      "\"updates\": [{" +
      "    \"date\": \"2020-01-11T00:00:00+00:00\"," +
      "    \"measure\": 10" +
      "},{" +
      "    \"date\": \"2020-01-12T00:00:00+00:00\"," +
      "    \"weight\": 501" +
      "},{" +
      "			\"date\": \"2020-01-13T00:00:00+00:00\"," +
      "     \"coaching\": 50" +
      "},{" +
      "		\"date\": \"2020-01-15T00:00:00+00:00\"," +
      "    \"coaching\": 50," +
      "    \"measure\": 10," +
      "    \"weight\": 105 }]}]";

    public const string SubmissionUpdateMinWeight =
      "[{" +
      "\"ubrn\": \"777666555437\"," +
      "\"type\": \"update\"," +
      "\"updates\": [{" +
      "    \"date\": \"2020-01-11T00:00:00+00:00\"," +
      "    \"measure\": 10" +
      "},{" +
      "    \"date\": \"2020-01-12T00:00:00+00:00\"," +
      "    \"weight\": 34" +
      "},{" +
      "			\"date\": \"2020-01-13T00:00:00+00:00\"," +
      "     \"coaching\": 50" +
      "},{" +
      "		\"date\": \"2020-01-15T00:00:00+00:00\"," +
      "    \"coaching\": 50," +
      "    \"measure\": 10," +
      "    \"weight\": 105 }]}]";


    public const string SubmissionCompletedWithUpdatesOutOfDate = "[{" +
"\"ubrn\": \"777666555444\"," +
"  \"date\": \"2020-01-18T00:00:00+00:00\"," +
"  \"type\": \"Completed\"," +
"  \"updates\": [" +
"      {" +
"          \"date\": \"2020-01-17T00:00:00+00:00\"," +
"          \"coaching\": 98," +
"          \"measure\": 9," +
"          \"weight\": 95.12 }]}]";

    public const string SubmissionDeclined = "[{" +
     "  \"ubrn\": \"000000000002\"," +
     "  \"date\": \"2020-01-18T00:00:00+00:00\"," +
     "  \"type\": \"Declined\"," +
     "  \"reason\": \"Reason why the service user was terminated " +
     "during programme\"	}]";
  }
}

namespace WmsHub.Business.Enums
{
  public enum UpdateType
  {
    NotValid,

    // Reject the service user from starting their selected programme.
    Rejected,

    // Provide the start date of the service user on their selected programme.
    Started,

    // Provide the date service user was conatcted.
    Contacted,

    // (Optional) Removes the service user from the Get Service User List
    // response before they have started the programme to shorten the list
    // of service users returned.
    Accepted,

    // Provide updates for the self-reported weight, engagement measure and 
    // for level 2 and 3 service users their coaching time throughout 
    // the programme.
    Update,

    //Terminate the patient after they have started their programme.
    Terminated,

    // Provide the completed date of the service user on their 
    // selected programme.
    Completed,
    
    // The service users decline the programme before starting it.
    Declined
  }
}

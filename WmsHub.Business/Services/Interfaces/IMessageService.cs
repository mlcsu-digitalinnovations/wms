using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WmsHub.Business.Entities;
using WmsHub.Business.Enums;
using WmsHub.Business.Models.MessageService;

namespace WmsHub.Business.Services;

public interface IMessageService: IServiceBase
{

  /// <summary>
  /// Adds a referral to the message queue ready to be sent.
  /// The message type is set by the calling method of Email, Letter or SMS.
  /// Letter is in the message type as a legacy call and future 
  /// use if required.
  /// </summary>
  /// <param name="referral">Entities.Referral</param>
  /// <param name="messageType">
  /// Enum.MessageType - Email, Letter or SMS.
  /// </param>
  /// <returns></returns>
  void AddReferralToMessageQueue(QueueItem queueItem, MessageType messageType);

  /// <summary>
  /// Adds a referral to the TextMessages table which records messages sent
  /// for first and second contact.  This also generates a link for the 
  /// service user to navigate to complete the sign up process.<br/>
  /// <list type="bullet">
  ///     <item><description>GeneralReferralFirst</description></item>
  ///     <item><description>GeneralReferralSecond</description></item>
  ///     <item><description>ElectiveCareReferralFirst</description></item>
  ///     <item><description>ElectiveCareReferralSecond</description></item>
  ///     <item><description>GpReferralFirst</description></item>
  ///     <item><description>GpReferralSecond</description></item>
  ///     <item><description>MskReferralFirst</description></item>
  ///     <item><description>MskReferralSecond</description></item>
  ///     <item><description>PharmacyReferralFirst</description></item>
  ///     <item><description>PharmacyReferralFirst</description></item>
  ///     <item><description>PharmacyReferralSecond</description></item>
  ///     <item><description>StaffReferralFirstMessage</description></item>
  ///     <item><description>StaffReferralSecondMessage</description></item>
  /// </list>
  /// </summary>
  /// <param name="item">QueueItem</param>
  /// <param name="type"> Enum.MessageType - Email, Letter or SMS</param>
  /// <returns></returns>
  Task<string> AddReferralToTextMessage(QueueItem item, MessageType type);

  /// <summary>
  /// Adds a message to the queue when an Elective Care User is created.
  /// </summary>
  /// <param name="principalId">Generated GUID to track the MessageQueue 
  /// and the Elective Care User.  Created once a used has been created
  /// on the MS Graph API.</param>
  /// <param name="odsCode">Organisation Code</param>
  /// <param name="emailAddress"></param>
  /// <returns>Password</returns>
  MessageQueue CreateElectiveCareUserCreateMessage(
    Guid principalId,
    string odsCode,
    string emailAddress);

  /// <summary>
  /// Adds an email message to the queue to send to the user that their 
  /// account has been removed.
  /// </summary>
  /// <param name="principalId">Generated GUID to track the MessageQueue 
  /// and the Elective Care User.  Created once a used has been created
  /// on the MS Graph API.</param>
  /// <param name="odsCode">Organisation Code</param>
  /// <param name="emailAddress"></param>
  /// <returns></returns>
  MessageQueue CreateElectiveCareUserDeleteMessage(
    Guid principalId,
    string odsCode,
    string emailAddress);

  /// <summary>
  /// Updates the status of each referral to FailedToContactTextMessage
  /// or FailedToContactEmailMessage where the referral has a 
  /// FailedToContact status.
  /// </summary>
  /// <returns>
  /// string detailing number of referrals updated and the Id's
  /// </returns>
  Task<string[]> PrepareFailedToContactAsync();

  /// <summary>
  /// Updates the status of each referral from New to TextMessage1.
  /// </summary>
  /// <returns>
  /// string detailing number of referrals updated and the Id's
  /// </returns>
  Task<string[]> PrepareNewReferralsToContactAsync();

  /// <summary>
  /// Updates the status of each referral from TExtMessage1 to TextMessage2 
  /// when the SentDate is not null.
  /// </summary>
  /// <returns>
  /// string detailing number of referrals updated and the Id's
  /// </returns>
  Task<string[]> PrepareTextMessage1ReferralsToContactAsync();


  /// <summary>
  /// MessageOptions contains StatusFilterFlag, which is used to filter the
  /// referrals based on the ReferralStatus flag value.
  /// 
  /// The ReferralStatus attribute extension ReferralStatusMessageType is
  /// used to set the message type.
  /// </summary>
  /// <returns>Number of messages queued grouped by Message Type.</returns>
  Task<Dictionary<string, string>> QueueMessagesAsync(
    bool sendFailedOnly = false);

  /// <summary>
  /// Saves a message to the queue once a user has been created.
  /// </summary>
  /// <param name="message">MessageQueue</param>
  /// <returns></returns>
  Task SaveElectiveCareMessage(MessageQueue message);

  /// <summary>
  /// Loops the message queue where the SentDate is NULL.  Once a message has 
  /// been sent and a valid response received, the SentDate is set.
  /// </summary>
  /// <returns>Number of messages sent grouped by Message Type.</returns>
  Task<Dictionary<string,string>> SendQueuedMessagesAsync();
}

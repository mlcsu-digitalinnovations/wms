using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using WmsHub.Business.Models;

namespace WmsHub.Business.Helpers
{
    public static class DischargeLetterCreator
    {
       public static byte[] GenerateDischargeLetter(Referral referral)
       {
          byte[] documentBytes = Array.Empty<byte>();

          PdfDocumentBuilder builder = new PdfDocumentBuilder();
          PdfPageBuilder page = builder.AddPage(PageSize.A4);
          PdfDocumentBuilder.AddedFont font =
            builder.AddStandard14Font(Standard14Font.Helvetica);
          PdfDocumentBuilder.AddedFont bold =
            builder.AddStandard14Font(Standard14Font.HelveticaBold);

          double margin = 45;
          double rightEdge = page.PageSize.Width - margin;
          double topEdge = page.PageSize.Height - margin;
          int lineHeight = 16;
          int heightOfLogo = 40;
          double lineOffsetY = 0;
          IReadOnlyList<Letter> letters = null;
          PdfPoint pageTopLeft = new PdfPoint(margin, topEdge);

          builder.DocumentInformation.Author =
            "MLCSU Referral Management Centre";
          builder.DocumentInformation.Title =
            "Weight Management Programme Discharge Letter";

          // logo
          var logoPlacement = new PdfRectangle(
            new PdfPoint(rightEdge - 100, topEdge - heightOfLogo),
            new PdfPoint(rightEdge, topEdge));
          page.AddPng(File.ReadAllBytes("./data/NHS.png"), logoPlacement);

          // sending address
          string today = DateTimeOffset.Now.ToString("dd MMM yyyy");
          string[] senderDetails = {
            "MLCSU Referral Management Centre",
            "Completely made up office name",
            "17 Any Street",
            "Any Town",
            "County",
            "Postcode",
            " ",
            "Tel: 01234 567890",
            " ",
            $"Date: {today}" };

          for (int i=0; i<senderDetails.Length; i++)
          {
            PdfRectangle boundingRectangle =
              CalculateBoundingBox(page, senderDetails[i], font);
            double startX = rightEdge - boundingRectangle.Width;
            double startY = topEdge - heightOfLogo - ((i+2) * lineHeight);

            letters = page.AddText(
              senderDetails[i], 12, new PdfPoint(startX, startY), font);

            lineOffsetY =
              letters.Min(x => x.GlyphRectangle.Bottom) - lineHeight;
          }

          // receiving address
          // lineOffsetY determines where the line will start
          if(!string.IsNullOrEmpty(referral.Address1))
          {
            lineOffsetY -= (2 * lineHeight);
            page.AddText(referral.Address1, 12,
              new PdfPoint(margin, margin + lineOffsetY), font);
            lineOffsetY -= lineHeight;
          }
          if(!string.IsNullOrEmpty(referral.Address2))
          {
            page.AddText(referral.Address2, 12,
              new PdfPoint(margin, margin + lineOffsetY), font);
            lineOffsetY -= lineHeight;
          }
          if(!string.IsNullOrEmpty(referral.Address3))
          {
            page.AddText(referral.Address3, 12,
              new PdfPoint(margin, margin + lineOffsetY), font);
            lineOffsetY -= lineHeight;
          }
          if(!string.IsNullOrEmpty(referral.Postcode))
          {
            page.AddText(referral.Postcode, 12,
              new PdfPoint(margin, margin + lineOffsetY), font);
            lineOffsetY -= lineHeight;
          }

          lineOffsetY -= lineHeight;
          page.AddText($"NHS NUMBER: {referral.NhsNumber}", 12,
            new PdfPoint(margin, margin + lineOffsetY), bold);

          lineOffsetY -= (2 * lineHeight);
          page.AddText($"Dear {referral.GivenName} {referral.FamilyName}", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= (2 * lineHeight);
          page.AddText("Lorem ipsum dolor sit amet, consectetur adipiscing " +
            "elit. Donec dapibus dictum rhoncus.", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= lineHeight;
          page.AddText("Etiam aliquet est in quam rutrum, nec bibendum nibh " +
            "efficitur. Ut rhoncus sollicitudin molestie.", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= lineHeight;
          page.AddText("Integer augue orci, porttitor id nibh nec, sagittis " +
            "semper nibh. Integer vel eu.", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= (2 * lineHeight);
          page.AddText($"Service provider: {referral.Provider.Name}", 12,
            new PdfPoint(margin, margin + lineOffsetY), bold);

          lineOffsetY -= (2 * lineHeight);
          page.AddText("Yours sincerely", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= (4 * lineHeight);
          page.AddText("Space for a signature ?", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          lineOffsetY -= (2 * lineHeight);
          page.AddText("Footer ?", 12,
            new PdfPoint(margin, margin + lineOffsetY), font);

          documentBytes = builder.Build();

         return documentBytes;
       }

       private static PdfRectangle CalculateBoundingBox(PdfPageBuilder page,
        string text, PdfDocumentBuilder.AddedFont font)
       {
          IReadOnlyList<Letter> letters =
            page.MeasureText(text, 12, new PdfPoint(0,0), font);

          Letter firstLetter = letters[0];
          Letter lastLetter = letters[letters.Count - 1];

          return new PdfRectangle(
            new PdfPoint(firstLetter.GlyphRectangle.BottomLeft.X,
              firstLetter.GlyphRectangle.BottomLeft.Y),
            new PdfPoint(lastLetter.GlyphRectangle.TopRight.X,
              lastLetter.GlyphRectangle.TopRight.Y));
       }
    }
}
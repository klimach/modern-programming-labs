using System.Text;
using OnlineStore.App.Utils;
using OnlineStore.Domain;

var output = new StringBuilder()
    .AppendLine("Modern Programming Course")
    .AppendLine()
    .AppendLine($"Student: {StudentInfo.FullName}")
    .AppendLine($"Group: {StudentInfo.Group}")
    .AppendLine()
    .AppendLine($"Variant: {StudentInfo.Variant}")
    .AppendLine($"Domain: {StudentInfo.Domain}")
    .AppendLine()
    .AppendLine($".NET: {FrameworkUtils.GetDotNetVersion()}");

Console.Write(output);

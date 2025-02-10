// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
  "Naming", 
  "CA1707:Identifiers should not contain underscores", 
  Justification = "Unit test methods can include underscores.", 
  Scope = "module")]

[assembly: SuppressMessage(
  "Style",
  "IDE0022:Use expression body for method",
  Justification = "Should not use expression body for unit test methods")]
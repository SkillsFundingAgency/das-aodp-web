﻿using System.CodeDom;

namespace Authentication;
static class RoleConstants
{
    public const string QFAUApprover = "qfau_user_approver";
    public const string QFAUReviewer = "qfau_user_reviewer";
    public const string IFATEReviewer = "ifate_user_reviewer";
    public const string OFQUALReviewer = "ofqual_user_reviewer";
    public const string AOApply = "ao_user";
    public const string QFAUFormBuilder = "qfau_admin_form_editor";
    public const string IFATEFormBuilder = "ifate_admin_form_editor";
    public const string QFAUImport = "qfau_admin_data_importer";
}
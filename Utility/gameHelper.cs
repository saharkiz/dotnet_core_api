using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using myvapi.Utility;

public static class gameHelper
{
    public static void givePoint(string connection, string type, string action, string userID, string referece, string description)
    {
        try
        {
            SqlParameter[] paramTemp = {
                    new SqlParameter("@type",type),
                    new SqlParameter("@userID",userID),
                    new SqlParameter("@referece",referece),
                    new SqlParameter("@action",action),
                    new SqlParameter("@status","success"),
                    new SqlParameter("@newPoint","0"),
                    new SqlParameter("@willmultiply","1"),
                    new SqlParameter("@description",description),
                };

            SqlHelper.ExecuteProcedureReturnString(connection,"udp_Points",paramTemp);
        }
        catch (Exception)
        {
        }
    }

}
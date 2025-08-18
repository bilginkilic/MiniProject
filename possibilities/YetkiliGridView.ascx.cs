using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;

namespace AspxExamples
{
    public partial class YetkiliGridView : System.Web.UI.UserControl
    {
        // Events
        public event EventHandler<YetkiliEventArgs> YetkiliSelected;
        public event EventHandler<YetkiliEventArgs> YetkiliDeleted;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        public void BindGrid()
        {
            var yetkiliList = Session["YetkiliList"] as List<YetkiliKayit>;
            if (yetkiliList == null)
            {
                yetkiliList = new List<YetkiliKayit>();
            }

            grdYetkililer.DataSource = yetkiliList;
            grdYetkililer.DataBind();
        }

        protected void grdYetkililer_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var yetkili = e.Row.DataItem as YetkiliKayit;
                if (yetkili != null)
                {
                    // İmza preview'larını ayarla
                    for (int i = 0; i < 3; i++)
                    {
                        var preview = e.Row.FindControl($"imgSignature{i + 1}_{yetkili.YetkiliKontakt}") as System.Web.UI.HtmlControls.HtmlGenericControl;
                        var hdnImza = e.Row.FindControl($"hdnImza{i + 1}") as HiddenField;
                        
                        if (preview != null && hdnImza != null && yetkili.Imzalar != null && yetkili.Imzalar.Count > i)
                        {
                            preview.Style["background-image"] = $"url('{yetkili.Imzalar[i].Base64Image}')";
                        }
                    }

                    // Row click event
                    e.Row.Attributes["onclick"] = $"selectRow(this, '{yetkili.YetkiliKontakt}');";
                }
            }
        }

        protected void grdYetkililer_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int index = Convert.ToInt32(e.CommandArgument);
                var yetkiliList = Session["YetkiliList"] as List<YetkiliKayit>;

                if (yetkiliList != null && index >= 0 && index < yetkiliList.Count)
                {
                    var yetkili = yetkiliList[index];

                    switch (e.CommandName)
                    {
                        case "EditRow":
                            YetkiliSelected?.Invoke(this, new YetkiliEventArgs { Yetkili = yetkili });
                            break;

                        case "DeleteRow":
                            yetkiliList.RemoveAt(index);
                            Session["YetkiliList"] = yetkiliList;
                            YetkiliDeleted?.Invoke(this, new YetkiliEventArgs { Yetkili = yetkili });
                            BindGrid();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Grid command error: {ex.Message}");
                throw;
            }
        }

        protected void grdYetkililer_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdYetkililer.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void grdYetkililer_Sorting(object sender, GridViewSortEventArgs e)
        {
            var yetkiliList = Session["YetkiliList"] as List<YetkiliKayit>;
            if (yetkiliList != null)
            {
                string sortDirection = ViewState["SortDirection"]?.ToString() ?? "ASC";
                ViewState["SortDirection"] = sortDirection == "ASC" ? "DESC" : "ASC";

                yetkiliList = sortDirection == "ASC" ?
                    yetkiliList.OrderBy(y => GetPropertyValue(y, e.SortExpression)).ToList() :
                    yetkiliList.OrderByDescending(y => GetPropertyValue(y, e.SortExpression)).ToList();

                Session["YetkiliList"] = yetkiliList;
                BindGrid();
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null) ?? "";
        }

        protected string GetImzaValue(object dataItem, int index)
        {
            var yetkili = dataItem as YetkiliKayit;
            if (yetkili?.Imzalar != null && yetkili.Imzalar.Count > index)
            {
                return yetkili.Imzalar[index].Base64Image;
            }
            return string.Empty;
        }

        public void AddYetkili(YetkiliKayit yetkili)
        {
            var yetkiliList = Session["YetkiliList"] as List<YetkiliKayit> ?? new List<YetkiliKayit>();
            yetkiliList.Add(yetkili);
            Session["YetkiliList"] = yetkiliList;
            BindGrid();
        }

        public void UpdateYetkili(YetkiliKayit yetkili)
        {
            var yetkiliList = Session["YetkiliList"] as List<YetkiliKayit>;
            if (yetkiliList != null)
            {
                var index = yetkiliList.FindIndex(y => y.YetkiliKontakt == yetkili.YetkiliKontakt);
                if (index >= 0)
                {
                    yetkiliList[index] = yetkili;
                    Session["YetkiliList"] = yetkiliList;
                    BindGrid();
                }
            }
        }

        public List<YetkiliKayit> GetYetkiliList()
        {
            return Session["YetkiliList"] as List<YetkiliKayit> ?? new List<YetkiliKayit>();
        }
    }

    public class YetkiliEventArgs : EventArgs
    {
        public YetkiliKayit Yetkili { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobinhoodDesktop
{
    public class SearchList : Panel
    {

        public SearchList()
        {
            SearchboxText = new TextBox();
            SearchboxText.Size = new System.Drawing.Size(this.Width - 20, 15);
            SearchboxText.Location = new System.Drawing.Point(10, 5);
            SearchboxText.TextChanged += SearchboxText_HandleKeypress;
            this.Controls.Add(SearchboxText);

            this.BackColor = System.Drawing.Color.FromArgb(90, 10, 10, 10);
            this.AutoScroll = true;
        }

        #region Types
        public class StockListItem : Panel
        {
            /// <summary>
            /// The label representing the stock
            /// </summary>
            public Label SymbolLabel;

            /// <summary>
            /// The label respresenting the full stock name
            /// </summary>
            public Label StockLabel;

            /// <summary>
            /// The label representing the current stock price
            /// </summary>
            public Label PriceLabel;

            /// <summary>
            /// The button used to add the stock to the watchlist
            /// </summary>
            public PictureBox AddButton;

            public StockListItem(string symbol, string name)
            {
                this.SymbolLabel = new Label();
                this.SymbolLabel.Size = new System.Drawing.Size(50, 15);
                this.SymbolLabel.Location = new System.Drawing.Point(5, 5);
                this.SymbolLabel.Text = symbol;
                Controls.Add(SymbolLabel);

                this.StockLabel = new Label();
                this.StockLabel.Size = new System.Drawing.Size(150, 15);
                this.StockLabel.Location = new System.Drawing.Point(this.SymbolLabel.Location.X + this.SymbolLabel.Width + 5, this.SymbolLabel.Location.Y);
                this.StockLabel.Text = name;
                Controls.Add(StockLabel);

                this.AddButton = new PictureBox();
                this.AddButton.Location = new System.Drawing.Point(this.StockLabel.Location.X + this.StockLabel.Width + 5, this.StockLabel.Location.Y);
                this.AddButton.Size = new System.Drawing.Size(30, 30);
                //this.AddButton.Image
                this.AddButton.MouseUp += (object sender, MouseEventArgs e) =>
                {

                };
                Controls.Add(this.AddButton);

                this.Size = new System.Drawing.Size(this.AddButton.Location.X + this.AddButton.Width + 5, this.AddButton.Height);
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// The textbox used for search input
        /// </summary>
        public TextBox SearchboxText;

        /// <summary>
        /// The current search results
        /// </summary>
        private List<StockListItem> SearchResults = new List<StockListItem>();
        #endregion

        /// <summary>
        /// Performs a search on symbols close to the specified string
        /// </summary>
        /// <param name="symbol"></param>
        public void Search(string symbol)
        {
            DataAccessor.Search(symbol, (Dictionary<string, string> r) => 
                {
                    this.BeginInvoke((Action<Dictionary<string, string>>)((Dictionary<string, string> results) =>
                    {
                        ClearSearchResults();
                        int yPos = 25;
                        foreach(var pair in results)
                        {
                            StockListItem item = new StockListItem(pair.Key, pair.Value);
                            item.Location = new System.Drawing.Point(5, yPos);
                            Controls.Add(item);

                            yPos += 35;
                        }
                    }), new object[] { r });
                    
                });
        }

        /// <summary>
        /// Clears any previous search results
        /// </summary>
        private void ClearSearchResults()
        {
            List<Control> savedControls = new List<Control>()
            {
                SearchboxText
            };

            // Remove all but the saved controls
            for(int i = 0; i < Controls.Count; i++)
            {
                if(!savedControls.Contains(Controls[i]))
                {
                    Controls.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Handles a keypress in the searchbox
        /// </summary>
        /// <param name="sender">The searchbox object</param>
        /// <param name="e">Information on the keys that are pressed</param>
        private void SearchboxText_HandleKeypress(Object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(SearchboxText.Text))
            {
                Search(SearchboxText.Text);
            }
            else
            {
                ClearSearchResults();
            }
        }

        
    }
}

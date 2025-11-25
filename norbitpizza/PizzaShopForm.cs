using System;
using System.Drawing;
using System.Windows.Forms;

namespace norbitpizza
{
    /// <summary>
    /// Основная форма приложения — адаптивный интерфейс пиццерии.
    /// </summary>
    public class PizzaShopForm : Form
    {
        // Поля интерфейса
        private FlowLayoutPanel _pizzaPanel;
        private Panel _cartPanel;
        private Button _cartButton;
        private Label _totalLabel;
        private Panel _specialOfferTile;

        public PizzaShopForm()
        {
            this.Text = "НорбитПицца";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Resize += PizzaShopForm_Resize;

            InitializeInterface();
        }

        private void InitializeInterface()
        {
            // === Верхняя панель (заголовок + корзина) ===
            Panel topPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            Label titleLabel = new Label
            {
                Text = "🍕 НорбитПицца",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(20, 15),
                AutoSize = true
            };

            _cartButton = new Button
            {
                Width = 50,
                Height = 40,
                Location = new Point(this.ClientSize.Width - 70, 10),
                BackColor = Color.FromArgb(50, 50, 50),
                FlatStyle = FlatStyle.Flat,
                Text = "🛒",
                Font = new Font("Segoe UI", 14),
                UseVisualStyleBackColor = false
            };
            _cartButton.Click += (s, e) => _cartPanel.Visible = !_cartPanel.Visible;

            topPanel.Controls.Add(titleLabel);
            topPanel.Controls.Add(_cartButton);

            // === Панель корзины (справа) ===
            _cartPanel = new Panel
            {
                Width = 300,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10),
                Visible = false
            };

            _cartPanel.Controls.Add(new Label
            {
                Text = "🛒 Корзина",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Orange,
                Dock = DockStyle.Top,
                Height = 30
            });

            _totalLabel = new Label
            {
                Text = "Итого: 0 ₽",
                ForeColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };
            _cartPanel.Controls.Add(_totalLabel);

            // === Основной контент: спецпредложение + пиццы ===
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            // Спецпредложение — сверху, не скроллится
            _specialOfferTile = CreateSpecialOfferTile();
            _specialOfferTile.Dock = DockStyle.Top;

            // Пиццы — прокручиваемая область под спецпредложением
            _pizzaPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            CreatePizzaTiles();

            // ВАЖНО: сначала Fill, потом Top — чтобы спецпредложение наложилось сверху
            contentPanel.Controls.Add(_pizzaPanel);
            contentPanel.Controls.Add(_specialOfferTile);

            // === Сборка формы ===
            this.Controls.Add(_cartPanel);    // Справа
            this.Controls.Add(contentPanel);  // Центр
            this.Controls.Add(topPanel);      // Сверху
        }

        private Panel CreateSpecialOfferTile()
        {
            Panel tile = new Panel
            {
                Height = 120,
                Margin = new Padding(15, 0, 15, 10),
                BackColor = Color.FromArgb(50, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            Button closeButton = new Button
            {
                Text = "×",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.LightGray,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(26, 26),
                Location = new Point(tile.Width - 32, 4),
                Cursor = Cursors.Hand
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 40, 40);
            closeButton.Click += (s, e) => tile.Visible = false;

            tile.Controls.Add(new Label
            {
                Text = "🔥",
                Font = new Font("Segoe UI", 20),
                ForeColor = Color.OrangeRed,
                Location = new Point(15, 20),
                AutoSize = true
            });

            tile.Controls.Add(new Label
            {
                Text = "СПЕЦПРЕДЛОЖЕНИЕ НЕДЕЛИ!",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(60, 20),
                AutoSize = true
            });

            tile.Controls.Add(new Label
            {
                Text = "Пицца 'Мега-Пепперони' всего за 399₽ вместо 650₽!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(60, 50),
                AutoSize = true
            });

            tile.Controls.Add(new Label
            {
                Text = "399 ₽",
                ForeColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(60, 75),
                AutoSize = true
            });

            Button buyBtn = new Button
            {
                Text = "Заказать",
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 26),
                Location = new Point(200, 75)
            };
            buyBtn.Click += (s, e) =>
                MessageBox.Show("Спецпредложение добавлено в корзину!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            tile.Controls.Add(buyBtn);
            tile.Controls.Add(closeButton);

            tile.Resize += (s, e) => closeButton.Location = new Point(tile.Width - 32, 4);
            tile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            return tile;
        }

        private void CreatePizzaTiles()
        {
            for (int i = 0; i < 5; i++)
            {
                Panel tile = new Panel
                {
                    Width = 220,
                    Height = 340,
                    Margin = new Padding(10),
                    BackColor = Color.FromArgb(40, 40, 40),
                    BorderStyle = BorderStyle.FixedSingle
                };

                PictureBox pb = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 200,
                    Height = 140,
                    Location = new Point(10, 10),
                    BackColor = Color.FromArgb(60, 60, 60)
                };
                pb.Controls.Add(new Label
                {
                    Text = "🖼️",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 20),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });

                tile.Controls.Add(pb);
                tile.Controls.Add(new Label { Text = $"Пицца {i + 1}", ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(10, 160) });
                tile.Controls.Add(new Label { Text = "Краткое описание вкусной пиццы", ForeColor = Color.Silver, Font = new Font("Segoe UI", 8.5F), AutoSize = true, Location = new Point(10, 185), MaximumSize = new Size(200, 0), AutoEllipsis = true });
                tile.Controls.Add(new Label { Text = "500 ₽", ForeColor = Color.Orange, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Location = new Point(10, 210) });

                NumericUpDown qty = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 1, Width = 60, Location = new Point(10, 245) };
                Button buy = new Button { Text = "Купить", Width = 90, Height = 26, Location = new Point(80, 245), BackColor = Color.Orange, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat };
                buy.Click += (s, e) => MessageBox.Show($"Куплено: {qty.Value} шт.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                tile.Controls.Add(qty);
                tile.Controls.Add(buy);

                _pizzaPanel.Controls.Add(tile);
            }
        }

        private void PizzaShopForm_Resize(object sender, EventArgs e)
        {
            _cartButton.Location = new Point(this.ClientSize.Width - _cartButton.Width - 20, 10);
        }
    }
}
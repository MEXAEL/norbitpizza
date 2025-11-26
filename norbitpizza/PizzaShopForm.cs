using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace norbitpizza
{
    public class PizzaItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class PizzaShopForm : Form
    {
        private FlowLayoutPanel _pizzaPanel;
        private Panel _cartPanel;
        private Button _cartButton;
        private Label _totalLabel;
        private Panel _specialOfferTile;
        private PictureBox _offerImageBox;
        private Label _offerHeaderLabel;
        private Label _offerDescLabel;
        private Label _offerPriceLabel;
        private Timer _offerTimer;
        private int _currentOfferIndex = 0;
        private List<PizzaItem> _cartItems = new List<PizzaItem>();
        private FlowLayoutPanel _cartItemsPanel;

        private readonly (string imageFile, string header, string description, string price)[] _offers = new[]
        {
            ("offer1.jpg", "ПИЦЦА ДЕНЬ!", "Пицца 'Маргарита' всего за 299₽", "299 ₽"),
            ("offer2.jpg", "СЕМЕЙНЫЙ НАБОР", "Большая пицца + 2 напитка за 599₽", "599 ₽"),
            ("offer3.jpg", "НОВИНКА!", "Пицца с трюфелем и моцареллой", "799 ₽")
        };

        // 🔴 Реальные пиццы и напитки — 25 штук (цены как decimal!)
        private readonly (string name, string desc, decimal price)[] _menuItems = new[]
        {
            ("Маргарита", "Классика: томатный соус, моцарелла и свежий базилик", 499m),
            ("Пепперони", "Острая пепперони, томатный соус и моцарелла", 599m),
            ("Гавайская", "Ветчина, сочные ананасы и моцарелла", 549m),
            ("Четыре сыра", "Моцарелла, дорблю, пармезан и гауда — для ценителей сыра", 649m),
            ("Мясная", "Пепперони, ветчина, курица, помидоры и моцарелла", 699m),
            ("Грибная", "Свежие шампиньоны, лесные грибы, моцарелла и чесночный соус", 599m),
            ("Овощная", "Болгарский перец, цукини, помидоры, оливки и моцарелла", 579m),
            ("Калифорния", "Курица, ананасы, кукуруза и моцарелла", 629m),
            ("Диабло", "Острая пепперони, перец чили, томатный соус и моцарелла", 649m),
            ("Барбекю", "Курица, лук, помидоры, соус BBQ и моцарелла", 679m),
            ("Морская", "Креветки, кальмары, томатный соус и моцарелла", 799m),
            ("Трюфельная", "Трюфельный соус, ветчина, артишоки и моцарелла", 899m),
            ("Карбонара", "Бекон, сливочный соус, моцарелла и яйцо", 699m),
            ("Французская", "Курица, шампиньоны, сливочный соус и моцарелла", 659m),
            ("Веганская", "Томаты, шпинат, авокадо, тофу и соус песто", 629m),

            ("Coca-Cola (0.5л)", "Классическая газировка", 129m),
            ("Sprite (0.5л)", "Освежающий лимонно-лаймовый напиток", 129m),
            ("Fanta (0.5л)", "Апельсиновый вкус", 129m),
            ("BonAqua (0.5л)", "Питьевая вода без газа", 89m),
            ("Сок Добрый (0.2л)", "Яблочный, апельсиновый или мультифрукт", 119m),
            ("Лимонад Домашний", "Свежевыжатый лимонад с мятой", 149m),
            ("Морс Клюквенный", "Натуральный морс из свежей клюквы", 139m),
            ("Чай Ароматный", "Зелёный или чёрный чай с добавками", 99m),
            ("Кофе Эспрессо", "Крепкий итальянский кофе", 159m),
            ("Капучино", "Эспрессо с нежной молочной пеной", 179m)
        };

        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        public PizzaShopForm()
        {
            this.Text = "НорбитПицца";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5F);
            this.Resize += PizzaShopForm_Resize;
            this.FormClosed += (s, e) => _offerTimer?.Stop();

            InitializeInterface();
        }

        private void InitializeInterface()
        {
            // === Верхняя панель ===
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

            // === Панель корзины ===
            _cartPanel = new Panel
            {
                Width = 320,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10),
                Visible = false
            };

            Label cartHeader = new Label
            {
                Text = "🛒 Корзина",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Orange,
                Dock = DockStyle.Top,
                Height = 30
            };

            _totalLabel = new Label
            {
                Text = "Итого: 0 ₽",
                ForeColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight
            };

            _cartItemsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(25, 25, 25),
                Padding = new Padding(5),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            try { SetWindowTheme(_cartItemsPanel.Handle, "DarkMode_Explorer", null); } catch { }

            Button checkoutButton = new Button
            {
                Text = "Оформить заказ",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            checkoutButton.Click += (s, e) =>
            {
                if (_cartItems.Count == 0)
                    MessageBox.Show("Корзина пуста!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                {
                    MessageBox.Show($"Ваш заказ на {_cartItems.Count} позиций принят!\nИтого: {CalculateTotal():F0} ₽", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearCart();
                }
            };

            _cartPanel.Controls.Add(checkoutButton);
            _cartPanel.Controls.Add(_totalLabel);
            _cartPanel.Controls.Add(_cartItemsPanel);
            _cartPanel.Controls.Add(cartHeader);

            // === Спецпредложение (БЕЗ КНОПКИ ЗАКРЫТЬ) ===
            _specialOfferTile = new Panel
            {
                Height = 140,
                Dock = DockStyle.Top,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };

            _offerImageBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 120,
                Height = 120,
                Location = new Point(15, 10),
                BackColor = Color.FromArgb(30, 25, 15)
            };

            _offerHeaderLabel = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(145, 20),
                AutoSize = true
            };

            _offerDescLabel = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(145, 50),
                AutoSize = true
            };

            _offerPriceLabel = new Label
            {
                ForeColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(145, 75),
                AutoSize = true
            };

            Button buySpecialBtn = new Button
            {
                Text = "Заказать",
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 26),
                Location = new Point(145, 105)
            };
            buySpecialBtn.FlatAppearance.BorderSize = 0;
            buySpecialBtn.Click += (s, e) =>
            {
                AddToCart(new PizzaItem
                {
                    Name = "Спецпредложение",
                    Description = "Акция недели",
                    Price = 399m,
                    Quantity = 1
                });
                _cartPanel.Visible = true;
            };

            // 🔻 КНОПКА ЗАКРЫТЬ УДАЛЕНА

            _specialOfferTile.Controls.Add(_offerImageBox);
            _specialOfferTile.Controls.Add(_offerHeaderLabel);
            _specialOfferTile.Controls.Add(_offerDescLabel);
            _specialOfferTile.Controls.Add(_offerPriceLabel);
            _specialOfferTile.Controls.Add(buySpecialBtn);

            // === Панель пицц ===
            _pizzaPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(20, 20, 20),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            try { SetWindowTheme(_pizzaPanel.Handle, "DarkMode_Explorer", null); } catch { }

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, _pizzaPanel, new object[] { true });

            CreatePizzaTiles(_menuItems.Length); // 25 позиций

            Panel contentPanel = new Panel { Dock = DockStyle.Fill };
            contentPanel.Controls.Add(_pizzaPanel);
            contentPanel.Controls.Add(_specialOfferTile);

            this.Controls.Add(_cartPanel);
            this.Controls.Add(contentPanel);
            this.Controls.Add(topPanel);

            UpdateSpecialOffer();
            _offerTimer = new Timer { Interval = 30_000 };
            _offerTimer.Tick += (s, e) => UpdateSpecialOffer();
            _offerTimer.Start();
        }

        private void AddToCart(PizzaItem item)
        {
            _cartItems.Add(item);
            RefreshCart();
        }

        private void RefreshCart()
        {
            _cartItemsPanel.Controls.Clear();

            foreach (var item in _cartItems)
            {
                Panel itemPanel = new Panel
                {
                    Width = _cartItemsPanel.ClientSize.Width - 20,
                    Height = 100,
                    Margin = new Padding(0, 0, 0, 10),
                    BackColor = Color.FromArgb(40, 40, 40),
                    BorderStyle = BorderStyle.FixedSingle
                };

                PictureBox img = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Width = 80,
                    Height = 80,
                    Location = new Point(10, 10),
                    BackColor = Color.FromArgb(50, 50, 50)
                };
                img.Controls.Add(new Label
                {
                    Text = item.Name.Contains("Пицца") ||
                           item.Name.Contains("Маргарита") || item.Name.Contains("Пепперони") ||
                           item.Name.Contains("Четыре сыра") || item.Name.Contains("Мясная") ||
                           item.Name.Contains("Грибная") || item.Name.Contains("Овощная") ||
                           item.Name.Contains("Калифорния") || item.Name.Contains("Диабло") ||
                           item.Name.Contains("Барбекю") || item.Name.Contains("Морская") ||
                           item.Name.Contains("Трюфельная") || item.Name.Contains("Карбонара") ||
                           item.Name.Contains("Французская") || item.Name.Contains("Веганская")
                        ? "🍕" : "🥤",
                    ForeColor = Color.Orange,
                    Font = new Font("Segoe UI", 20),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });

                Label nameLabel = new Label
                {
                    Text = item.Name,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(100, 10),
                    AutoSize = true
                };

                Label descLabel = new Label
                {
                    Text = item.Description,
                    ForeColor = Color.Silver,
                    Font = new Font("Segoe UI", 8),
                    Location = new Point(100, 30),
                    AutoSize = true,
                    MaximumSize = new Size(180, 0),
                    AutoEllipsis = true
                };

                Label qtyLabel = new Label
                {
                    Text = $"x{item.Quantity}",
                    ForeColor = Color.LimeGreen,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(100, 50),
                    AutoSize = true
                };

                Label priceLabel = new Label
                {
                    Text = $"{item.Price * item.Quantity} ₽",
                    ForeColor = Color.Orange,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(100, 70),
                    AutoSize = true
                };

                Button removeBtn = new Button
                {
                    Text = "×",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.Red,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(24, 24),
                    Location = new Point(itemPanel.Width - 30, 4),
                    Cursor = Cursors.Hand
                };
                removeBtn.FlatAppearance.BorderSize = 0;
                removeBtn.Click += (s, e) => { _cartItems.Remove(item); RefreshCart(); };
                itemPanel.Resize += (s, e) => removeBtn.Location = new Point(itemPanel.Width - 30, 4);

                itemPanel.Controls.Add(img);
                itemPanel.Controls.Add(nameLabel);
                itemPanel.Controls.Add(descLabel);
                itemPanel.Controls.Add(qtyLabel);
                itemPanel.Controls.Add(priceLabel);
                itemPanel.Controls.Add(removeBtn);

                _cartItemsPanel.Controls.Add(itemPanel);
            }

            _totalLabel.Text = $"Итого: {CalculateTotal():F0} ₽";
        }

        private void ClearCart()
        {
            _cartItems.Clear();
            RefreshCart();
        }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (var item in _cartItems)
                total += item.Price * item.Quantity;
            return total;
        }

        private void UpdateSpecialOffer()
        {
            if (_offers.Length == 0) return;

            var offer = _offers[_currentOfferIndex];
            _currentOfferIndex = (_currentOfferIndex + 1) % _offers.Length;

            _offerHeaderLabel.Text = offer.header;
            _offerDescLabel.Text = offer.description;
            _offerPriceLabel.Text = offer.price;

            string imagePath = Path.Combine(Application.StartupPath, offer.imageFile);
            if (File.Exists(imagePath))
            {
                try
                {
                    _offerImageBox.Image?.Dispose();
                    _offerImageBox.Image = Image.FromFile(imagePath);
                    _offerImageBox.Controls.Clear();
                }
                catch
                {
                    ShowImagePlaceholder();
                }
            }
            else
            {
                ShowImagePlaceholder();
            }
        }

        private void ShowImagePlaceholder()
        {
            _offerImageBox.Image = null;
            _offerImageBox.Controls.Clear();
            _offerImageBox.Controls.Add(new Label
            {
                Text = "📷",
                ForeColor = Color.Gold,
                Font = new Font("Segoe UI", 24),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
        }

        private void CreatePizzaTiles(int count)
        {
            _pizzaPanel.Controls.Clear();

            for (int i = 0; i < count; i++)
            {
                var item = _menuItems[i];
                string name = item.name;
                string desc = item.desc;
                decimal price = item.price;

                Panel tile = new Panel
                {
                    Height = 360,
                    Margin = new Padding(8),
                    BackColor = Color.FromArgb(40, 40, 40),
                    BorderStyle = BorderStyle.FixedSingle
                };

                PictureBox pb = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Height = 130,
                    Location = new Point(10, 10),
                    BackColor = Color.FromArgb(60, 60, 60)
                };
                pb.Controls.Add(new Label
                {
                    Text = name.Contains("Пицца") ||
                           name.Contains("Маргарита") || name.Contains("Пепперони") ||
                           name.Contains("Четыре сыра") || name.Contains("Мясная") ||
                           name.Contains("Грибная") || name.Contains("Овощная") ||
                           name.Contains("Калифорния") || name.Contains("Диабло") ||
                           name.Contains("Барбекю") || name.Contains("Морская") ||
                           name.Contains("Трюфельная") || name.Contains("Карбонара") ||
                           name.Contains("Французская") || name.Contains("Веганская")
                        ? "🍕" : "🥤",
                    ForeColor = Color.Orange,
                    Font = new Font("Segoe UI", 20),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });
                tile.Controls.Add(pb);

                // Название
                tile.Controls.Add(new Label
                {
                    Text = name,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Location = new Point(10, 150),
                    MaximumSize = new Size(200, 0),
                    AutoSize = true
                });

                // 🔽 Описание — через TextBox для WordWrap
                TextBox descBox = new TextBox
                {
                    Text = desc,
                    ForeColor = Color.Silver,
                    Font = new Font("Segoe UI", 8.5F),
                    Location = new Point(10, 175),
                    Size = new Size(200, 60),
                    Multiline = true,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(40, 40, 40),
                    TabStop = false,
                    WordWrap = true
                };
                tile.Controls.Add(descBox);

                // Цена
                tile.Controls.Add(new Label
                {
                    Text = $"{price} ₽",
                    ForeColor = Color.Orange,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(10, 240),
                    AutoSize = true
                });

                NumericUpDown qty = new NumericUpDown
                {
                    Minimum = 1,
                    Maximum = 20,
                    Value = 1,
                    Width = 60,
                    Location = new Point(10, 270)
                };
                tile.Controls.Add(qty);

                Button buy = new Button
                {
                    Text = "Купить",
                    Width = 90,
                    Height = 26,
                    Location = new Point(80, 270),
                    BackColor = Color.Red,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                buy.FlatAppearance.BorderSize = 0;
                buy.Click += (s, e) =>
                {
                    AddToCart(new PizzaItem { Name = name, Description = desc, Price = price, Quantity = (int)qty.Value });
                    _cartPanel.Visible = true;
                };
                tile.Controls.Add(buy);

                _pizzaPanel.Controls.Add(tile);
            }

            UpdateTileSizes();
        }

        private void UpdateTileSizes()
        {
            int availableWidth = this.ClientSize.Width - 20;
            if (availableWidth <= 0) return;

            const int minTileWidth = 200;
            const int maxTileWidth = 280;

            int columns = Math.Max(1, availableWidth / minTileWidth);
            int tileWidth = Math.Min(maxTileWidth, (availableWidth - (columns - 1) * 16) / columns);
            tileWidth = Math.Max(minTileWidth, tileWidth);

            foreach (Control ctl in _pizzaPanel.Controls)
            {
                if (ctl is Panel tile)
                {
                    tile.Width = tileWidth;

                    if (tile.Controls[0] is PictureBox pb)
                    {
                        pb.Width = tileWidth - 20;
                    }

                    // Обновляем размер описания (TextBox)
                    if (tile.Controls.Count > 2 && tile.Controls[2] is TextBox descBox)
                    {
                        descBox.Size = new Size(tileWidth - 20, 60);
                    }

                    if (tile.Controls.Count > 5)
                    {
                        tile.Controls[3].Location = new Point(10, 240);
                        tile.Controls[4].Location = new Point(10, 270);
                        tile.Controls[5].Location = new Point(Math.Max(80, tileWidth - 110), 270);
                    }
                }
            }

            _pizzaPanel.PerformLayout();
        }

        private void PizzaShopForm_Resize(object sender, EventArgs e)
        {
            _cartButton.Location = new Point(this.ClientSize.Width - _cartButton.Width - 20, 10);
            UpdateTileSizes();
        }
    }
}
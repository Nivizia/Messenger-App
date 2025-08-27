using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ApiFetcher;

namespace MessengerUI
{
    /// <summary>
    /// Interaction logic for ContactWindow.xaml
    /// </summary>
    public partial class ContactWindow : Window
    {

        public string token { get; set; } = string.Empty;

        public ObservableCollection<UserDto> SearchResults { get; } = new();



        public string Username = string.Empty;
        public ContactWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        /*private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchData = TxtGlobalSearch.Text?.Trim(); // Lấy dữ liệu tìm kiếm từ TextBox

            if (!string.IsNullOrEmpty(searchData)) return;
            
                LvSearch.ItemsSource = null;
            LvSearch.Items.Clear();

            // Gọi API để tìm kiếm người dùng
            var users = await SearchUsersAsync(searchData);

                // Cập nhật danh sách kết quả tìm kiếm vào ListView
                if (users != null && users.Any())
                {
                    LvSearch.ItemsSource = users;  // Hiển thị kết quả vào ListView
                }
                else
                {
                    MessageBox.Show("Không tìm thấy người dùng.");
                }
            
        }*/
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var keyword = TxtGlobalSearch.Text?.Trim();
            if (string.IsNullOrEmpty(keyword)) return;

            try
            {
                // gọi API
                var users = await ChatService.SearchUsersAsync(token, keyword);

                // làm mới kết quả: Clear + Add (KHÔNG đụng ItemsSource)
                SearchResults.Clear();
                if (users != null)
                    foreach (var u in users) SearchResults.Add(u);

                if (SearchResults.Count == 0)
                    MessageBox.Show("Không tìm thấy người dùng.");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Search lỗi: {ex.Message}");
            }
        }

        // Hàm gọi API để tìm kiếm người dùng
        private async Task<List<UserDto>> SearchUsersAsync(string searchData)
        {
            
            var users = await ChatService.SearchUsersAsync(token, searchData);
            return users;
        }


        /*private void LvSearch_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedUser = LvSearch.SelectedItem as UserDto;
            if (selectedUser != null)
            {
                MessageBox.Show($"You selected: {selectedUser.username}");
            }
        }*/

        private void LvSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvSearch.SelectedItem is UserDto u)
                MessageBox.Show($"You selected: {u.username}");
        }


        private void LvRecent_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Kiểm tra item được chọn
            var listView = sender as ListView;
            var selectedItem = listView?.SelectedItem;
            if (selectedItem != null)
            {
                // Ví dụ: ép kiểu về model RecentChatDto (bạn thay bằng class thật bạn đang dùng)
                // var chat = selectedItem as RecentChatDto;
                // MessageBox.Show($"Bạn đã double-click vào: {chat?.DisplayName}");

                MessageBox.Show("Bạn đã click double vào 1 item trong Recent Chats!");
            }
        }

        private async void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UserDto user)
            {
                try
                {
                    // Nếu chưa có conversation -> tạo mới
                    if (string.IsNullOrEmpty(user.ConversationId))
                    {
                        var ids = await ChatService.CreateConversationAsync(token, user.UserId); // = user._id
                        var convId = ids?.FirstOrDefault();

                        if (string.IsNullOrEmpty(convId))
                        {
                            MessageBox.Show("Không lấy được conversation id.");
                            return;
                        }
                        user.ConversationId = convId;
                    }
                    // Mở cửa sổ chat
                    ChattingWindow chatWin = new ChattingWindow(token, user.ConversationId!);
                    chatWin.Title = $"Chat with {user.username}";
                    chatWin.Show();
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot open chat: {ex.Message}");
                }
            }
        }




        // Sự kiện khi người dùng muốn làm mới danh sách recent chats
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var recentChats = await ChatService.GetUsersAsync(token); // Lấy danh sách recent chats
            if (recentChats != null && recentChats.Any())
            {
                LvRecent.ItemsSource = recentChats;  // Hiển thị kết quả vào ListView
            }
            else
            {
                MessageBox.Show("Không có cuộc trò chuyện gần đây.");
            }
        }

    }
}

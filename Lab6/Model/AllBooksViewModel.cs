using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Lab6
{
   
    public class  AllBooksViewModel :INotifyPropertyChanged
    {
        public ObservableCollection<Book> Books { get; set; }
        private Book selectedBook;
        public Book SelectedBook
        {
            get
            {
                return selectedBook;
            }

            set
            {
                selectedBook = value;
               OnPropertyChanged("SelectedBook");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private BaseCommand addCommand;
        public BaseCommand AddCommand
        {
            get
            {
                return addCommand ??
                (addCommand = new BaseCommand(obj =>
                {
                    Book book = new Book();
                    Books.Insert(0, book);
                    SelectedBook = book;
                }));
                
            }
        }
        private BaseCommand delCommand;
        public BaseCommand DelCommand
        {
            get
            {
                if (delCommand != null)
                    return delCommand;
                else
                {
                    Action<object> Execute = o =>
                    {
                        Book b = (Book)o;
                        Books.Remove(b);
                    };
                    Func<object, bool> CanExecute = o => Books.Count > 0;
                    delCommand = new BaseCommand(Execute, CanExecute);
                    return delCommand;
                }
            }
        }

        private BaseCommand saveCommand;
        public BaseCommand SaveCommand
        {
            get
            {
                if (saveCommand != null)
                    return saveCommand;
                else
                {
                    Action<object> Execute = o =>
                    {
                        SaveFileDialog sfd = new SaveFileDialog
                        {
                            InitialDirectory = Environment.CurrentDirectory,
                            Filter = "Файл в xml|*.xml|Файл в json|*.json"
                        };
                        if (sfd.ShowDialog() == true)
                        {
                            switch (sfd.FilterIndex)
                            {
                                case 1:
                                    {
                                        XElement x = new XElement("Books",
                                                  from book in Books
                                                  select new XElement("Book",
                                                         new XElement("Title", book.Title),
                                                         new XElement("Author", book.Author),
                                                         new XElement("Publisher", book.Publisher),
                                                         new XElement("Year", book.Year)));
                                        string s = x.ToString();
                                        FileStream fs = File.Create(sfd.FileName);
                                        StreamWriter sw = new StreamWriter(fs);
                                        sw.Write(s);
                                        sw.Close();
                                        fs.Close();
                                        break;
                                    }
                                case 2:
                                    {
                                        string s = JsonConvert.SerializeObject(Books, Newtonsoft.Json.Formatting.Indented);
                                        FileStream fs = File.Create(sfd.FileName);
                                        StreamWriter sw = new StreamWriter(fs);
                                        sw.Write(s);
                                        sw.Close();
                                        fs.Close();
                                        break;
                                    }
                            }
                        }
                    };
                    Func<object, bool> CanExecute = o => Books.Count > 0;
                    saveCommand = new BaseCommand(Execute, CanExecute);
                    return saveCommand;
                }
            }
        }

        //команда открытия файла
        private BaseCommand openCommand;
        public BaseCommand OpenCommand
        {
          
           get
           {
                return openCommand ??
                  (openCommand = new BaseCommand(obj =>
                  {
                      OpenFileDialog ofd = new OpenFileDialog
                      {
                          InitialDirectory = Environment.CurrentDirectory,
                          Filter = "Файл в xml|*.xml|Файл в json|*.json"
                      };
                      if (ofd.ShowDialog() == true)
                      {
                          switch (ofd.FilterIndex)
                          {
                              case 1:
                                  {
                                      using (Stream sw = new FileStream(ofd.FileName, FileMode.Open))
                                      {
                                          byte[] array = new byte[sw.Length];
                                          sw.Read(array, 0, array.Length);
                                          string s = Encoding.Default.GetString(array);
                                          var x = XElement.Parse(s);
                                          Books.Clear();
                                          foreach (var e in x.Elements())
                                          {
                                              Book book = new Book();
                                              if (!e.IsEmpty)
                                              {
                                                  try
                                                  {
                                                      if (e.Elements("Title").Any())
                                                      {
                                                          if (e.Element("Title").Value.Trim(' ') != "" && e.Element("Title").Value != null)
                                                          {
                                                              book.Title = e.Element("Title").Value;
                                                          }
                                                          else book.Title = "Underfinded";
                                                      }
                                                      else book.Title = "Underfinded";

                                                  }
                                                  catch (Exception)
                                                  {
                                                      book.Title = "Underfinded";
                                                      throw;
                                                  }
                                              }
                                              else book.Title = "Underfinded";
                                              try
                                              {

                                                  if (e.Elements("Author").Any())
                                                  {
                                                      if (e.Element("Author").Value.Trim(' ') != "")
                                                      {
                                                          book.Author = e.Element("Author").Value;
                                                      }
                                                      else book.Author = "Underfinded";
                                                  }
                                                  else book.Author = "Underfinded";
                                              }
                                              catch
                                              {
                                                  book.Author = "Underfinded";
                                              }

                                              try
                                              {
                                                  if (e.Elements("Publisher").Any())
                                                  {
                                                      if (e.Element("Publisher").Value.Trim(' ') != "")
                                                      {
                                                          book.Publisher = e.Element("Publisher").Value;
                                                      }
                                                      else book.Publisher = "Underfinded";
                                                  }
                                                  else book.Publisher = "Underfinded";
                                              }
                                              catch
                                              {
                                                  book.Publisher = "Underfinded";
                                              }

                                              try
                                              {
                                                  if (e.Elements("Year").Any())
                                                  {
                                                      if (e.Element("Year").Value.Trim(' ') != "")
                                                      {
                                                          book.Year = Int32.Parse(e.Element("Year").Value);
                                                      }
                                                      else book.Year = 0;
                                                  }else book.Year = 0;
                                              }
                                              catch
                                              {
                                                  book.Year = 0;
                                              }

                                              Books.Add(book);
                                          }
                                          //var q = from e in x.Elements()
                                          // select new Book { 
                                          //     Title = e.Element("Title").Value, 
                                          //     Author = e.Element("Author").Value, 
                                          //     Year = Convert.ToInt16(e.Element("Year").Value), 
                                          //     Publisher = e.Element("Publisher").Value                                            
                                          // };

                                          // foreach (var i in q)
                                          // {
                                          //  Books.Add(i);
                                          // }
                                      }
                                      break;
                                  }
                              case 2:
                                  {
                                      using (Stream sw = new FileStream(ofd.FileName, FileMode.Open))
                                      {
                                          byte[] array = new byte[sw.Length];
                                          sw.Read(array, 0, array.Length);
                                          string s = Encoding.Default.GetString(array);
                                          ObservableCollection<Book> jsonBooks = JsonConvert.DeserializeObject<ObservableCollection<Book>>(s);
                                          Books.Clear();
                                          foreach (var b in jsonBooks)
                                          {
                                              Book Book = new Book { Title = b.Title.ToString(), Author = b.Author.ToString(), Year = Convert.ToInt16(b.Year), Publisher = b.Publisher.ToString() };
                                              Books.Add(Book);
                                          }
                                      }
                                      break;
                                  }
                          }
                      }
                  }));
            }
            
        }
        

        public AllBooksViewModel()
        {
            Books = new ObservableCollection<Book>
 {
 new Book {Title="WPF Unleashed", Author="Adam Natan",Publisher = "GPO", Year=2012 },
 new Book {Title="F# for Machine Learning", Author="Sudipta Mukherjee", Publisher = "Rutgers University Press", Year=2016 },
 new Book {Title="F# for Fun and Profit", Author="Scott Wlaschin", Publisher = "GPO",Year=2015 },
 new Book {Title="Learning C# by Developing Games with Unity 3D", Author="Terry Norton", Publisher = "Yale University Press",Year=2013 },
 new Book {Title="To Kill a Mockingbird", Author="Harper Lee", Publisher = "Rutgers University Press",Year=1960},
 new Book {Title="1984", Author="George Orwell",Publisher = "Yale University Press", Year=1948 },
 new Book {Title="The Great Gatsby", Author="F. Scott Fitzgerald",Publisher = "GPO", Year=1925 },
new Book {Title="The Book Thief", Author="Markus Zusak", Publisher = "Yale University Press",Year=1939 }
 };
        }
    }




}

using UnityEngine;

namespace IkanPintar
{
    public class HelpScreen : GameScreen
    {
        public HelpScreen(GameApp app) : base(app)
        {
        }

        protected override void Build()
        {
            Ui.Picture(Root, "Bg", Art.Get("Bg Text Input"), 640f, 360f, 1280f, 720f, Color.white);
            Ui.Panel(Root, "Panel", Ui.Hex(0x04243F, 0.38f), 640f, 340f, 1080f, 540f);

            Ui.Label(Root, "Title", "Cara Bermain", 100, Color.white, TextAnchor.MiddleCenter, 640f, 100f, 900f, 130f, Art.Title, true);

            string body =
                "<b>Kuis Buah</b>\n" +
                "Lihat gambar buah, lalu pilih nama yang benar sebelum waktu habis. Klik pilihan atau tekan 1-4 (atau A-D). Jawaban cepat dan beruntun memberi bonus skor.\n\n" +
                "<b>Ikan Lapar</b>\n" +
                "Gerakkan ikan kecil dengan tombol panah atau WASD. Makan sebanyak mungkin makanan ikan dan hindari ikan besar. Kamu punya 3 nyawa dan waktu 60 detik.\n\n" +
                "<b>Ketik Cepat</b>\n" +
                "Ketik kalimat yang tampil dengan benar. Huruf yang salah berwarna merah, hapus dengan Backspace. Setiap kalimat selesai, ikan berenang mendekati makanannya.\n\n" +
                "Tekan Esc saat bermain untuk menjeda.";

            Ui.Label(Root, "Body", body, 25, Color.white, TextAnchor.UpperLeft, 640f, 345f, 980f, 470f);

            AddButton(Root, "Back", "Kembali", 640f, 650f, 320f, 66f, 30, ToMenu);
        }

        void ToMenu()
        {
            App.Go(() => new MenuScreen(App));
        }

        public override void Back()
        {
            ToMenu();
        }

        public override void Tick(float dt)
        {
            if (GameInput.WasPressed(GameKey.Enter) || GameInput.WasPressed(GameKey.Space)) ToMenu();
        }
    }
}

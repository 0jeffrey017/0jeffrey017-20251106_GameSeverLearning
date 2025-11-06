package main

import (
	"fmt"
	"log"
	"net/http"

	"github.com/gorilla/websocket"
)

var upgrader = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool { return true }, // 開發用：允許所有連線
}

// 儲存所有連線的 map
var clients = make(map[*websocket.Conn]bool)

func main() {
	// 設定 /ws 路徑的 handler
	http.HandleFunc("/ws", handleConnection)

	fmt.Println("Server started on :8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}

// 每當有 client 連線，就會呼叫這個 handler
func handleConnection(w http.ResponseWriter, r *http.Request) {
	// 將 HTTP 連線升級為 WebSocket
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("Upgrade error:", err)
		return
	}
	defer conn.Close()

	// 登錄 client
	clients[conn] = true
	defer delete(clients, conn)

	log.Println("Client connected:", conn.RemoteAddr())

	// 不斷讀取訊息並廣播給所有 client
	for {
		_, msg, err := conn.ReadMessage()
		if err != nil {
			log.Println("Client disconnected:", err)
			return
		}

		log.Println("Broadcast:", string(msg))
		for c := range clients {
			if err := c.WriteMessage(websocket.TextMessage, msg); err != nil {
				log.Println("Write error:", err)
				c.Close()
				delete(clients, c)
			}
		}
	}
}

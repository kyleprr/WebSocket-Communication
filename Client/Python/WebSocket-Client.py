import asyncio
import websockets

async def connect_to_server():
    uri = "ws://localhost:1234/"
    async with websockets.connect(uri) as websocket:
        while True:
            message = input("Enter a message to send to the server (or 'exit' to quit): ")
            if message == "exit":
                break
            
            await websocket.send('{"request":"%s","subrequest":"login","data":{"id":"1","firstName":"Kyle","lastName":"Pereira"}}' % message)

            response = await websocket.recv()
            print("Received from server:", response)

asyncio.get_event_loop().run_until_complete(connect_to_server())
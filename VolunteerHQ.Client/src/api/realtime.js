import * as signalR from '@microsoft/signalr'

let connection = null
let starting = null

function getConnection() {
  const token = localStorage.getItem('token')
  if (!token) return null

  if (connection) return connection

  connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat', {
      accessTokenFactory: () => localStorage.getItem('token') || '',
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  starting = connection.start().catch(err => {
    console.error('SignalR connection failed', err)
    connection = null
  })

  return connection
}

function subscribe(eventName, handler) {
  const conn = getConnection()
  if (!conn) return { stop: () => {} }

  conn.on(eventName, handler)

  return {
    stop: () => {
      try { conn.off(eventName, handler) } catch { void 0 }
    },
  }
}

export const connectMessages = (onMessage) =>
  subscribe('ReceiveMessage', onMessage)

export const connectNotifications = (onNotification) =>
  subscribe('ReceiveNotification', onNotification)

export function stopRealtime() {
  if (connection) {
    connection.stop().catch(() => {})
    connection = null
    starting = null
  }
}

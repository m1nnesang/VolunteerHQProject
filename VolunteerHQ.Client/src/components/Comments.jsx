import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { getComments, createComment, deleteComment } from '../api/comments'
import { useAuth } from '../context/AuthContext'

export default function Comments({ fundraiserId }) {
  const { user, isUser } = useAuth()
  const [comments, setComments] = useState([])
  const [loading, setLoading] = useState(true)
  const [text, setText] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const load = () => {
    setLoading(true)
    getComments(fundraiserId, 1, 50)
      .then(res => setComments(res.data?.items || []))
      .catch(console.error)
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [fundraiserId])

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!text.trim()) return
    setSubmitting(true)
    try {
      await createComment(fundraiserId, { text: text.trim() })
      setText('')
      load()
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка при додаванні коментаря')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDelete = async (commentId) => {
    if (!confirm('Видалити коментар?')) return
    try {
      await deleteComment(fundraiserId, commentId)
      setComments(prev => prev.filter(c => c.id !== commentId))
    } catch {
      toast.error('Помилка при видаленні коментаря')
    }
  }

  return (
    <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-6">
      <h3 className="text-white font-semibold mb-5">
        Коментарі {comments.length > 0 && <span className="text-gray-500 text-sm">({comments.length})</span>}
      </h3>

      {/* Форма */}
      {isUser ? (
        <form onSubmit={handleSubmit} className="mb-6">
          <textarea
            value={text}
            onChange={e => setText(e.target.value)}
            placeholder="Залишити коментар..."
            rows={3}
            maxLength={1000}
            className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors resize-none"
          />
          <div className="flex items-center justify-between mt-2">
            <span className="text-gray-600 text-xs">{text.length}/1000</span>
            <button
              type="submit"
              disabled={submitting || !text.trim()}
              className="bg-yellow-400 text-black text-sm font-semibold px-4 py-2 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {submitting ? 'Надсилаємо...' : 'Надіслати'}
            </button>
          </div>
        </form>
      ) : !user ? (
        <div className="bg-[#111218] border border-[#2e303a] rounded-lg p-4 mb-6 text-center">
          <p className="text-gray-500 text-sm">
            <Link to="/login" className="text-yellow-400 hover:text-yellow-300">Увійдіть</Link>
            {' '}щоб залишити коментар
          </p>
        </div>
      ) : null}

      {/* Список */}
      {loading ? (
        <div className="space-y-3">
          {[1, 2].map(i => <div key={i} className="bg-[#2e303a] rounded-lg h-16 animate-pulse" />)}
        </div>
      ) : comments.length === 0 ? (
        <div className="text-center text-gray-500 text-sm py-8">
          Ще немає коментарів. Будьте першим!
        </div>
      ) : (
        <div className="space-y-3">
          {comments.map(c => (
            <div key={c.id} className="bg-[#111218] border border-[#2e303a] rounded-lg p-4">
              <div className="flex items-start justify-between mb-2">
                <div className="flex items-center gap-2">
                  <div className="w-7 h-7 bg-yellow-400 rounded-full flex items-center justify-center text-black text-xs font-bold">
                    {c.firstName && c.secondName
                      ? `${c.firstName.charAt(0)}${c.secondName.charAt(0)}`.toUpperCase()
                      : '?'}
                  </div>
                  <div className="flex flex-col">
                    <span className="text-gray-300 text-sm font-medium leading-tight">
                      {c.firstName && c.secondName
                        ? `${c.firstName} ${c.secondName.charAt(0)}.`
                        : 'Анонім'}
                    </span>
                    <span className="text-gray-600 text-xs">
                      {new Date(c.createdAt).toLocaleDateString('uk-UA', {
                        day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'
                      })}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {isUser && c.userId && c.userId !== user?.userId && (
                    <Link
                      to={`/messages/${c.userId}`}
                      className="text-gray-600 hover:text-yellow-400 transition-colors text-xs"
                      title="Написати"
                    >
                      ✉
                    </Link>
                  )}
                  {user && c.userId === user.userId && (
                    <button
                      onClick={() => handleDelete(c.id)}
                      className="text-gray-600 hover:text-red-400 transition-colors text-xs"
                      title="Видалити"
                    >
                      ✕
                    </button>
                  )}
                </div>
              </div>
              <p className="text-gray-300 text-sm leading-relaxed whitespace-pre-wrap">{c.text}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

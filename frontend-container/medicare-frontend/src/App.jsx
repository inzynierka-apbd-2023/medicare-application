import { useState } from 'react'
import './styles/App.css'
import './styles/styles.css'
import Login from './pages/Login'
import PatientDashboard from './pages/Dashboard/PatientDashboard'

function App() {
  const [count, setCount] = useState(0)

  return (
    <div className="min-h-screen bg-gray-100">
      <PatientDashboard />
    </div>
  )
}

export default App

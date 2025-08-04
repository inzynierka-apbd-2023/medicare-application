import { useState } from 'react'
import './styles/App.css'
import Login from './pages/Login/Login'
import ForgotPassword from './pages/Login/ForgotPassword'
import ForgotCardNumber from './pages/Login/ForgotCardNumber'
import Register from './pages/Register'
import ChoosePlan from './pages/ChoosePlan'
import PlanSelection from './pages/PlanSelection'
import LoginSuccess from './pages/LoginSuccess'
import RegistrationSuccess from './pages/RegistrationSuccess'
import PasswordResetSuccess from './pages/PasswordResetSuccess'
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom'

function App() {
  const [count, setCount] = useState(0)

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/forgot-password" element={<ForgotPassword/>} />
        <Route path="/forgot-card" element={<ForgotCardNumber />} />
        <Route path="/choose-plan" element={<ChoosePlan />} />
        <Route path="/plan-selection" element={<PlanSelection />} />
        <Route path="/login-success" element={<LoginSuccess />} />
        <Route path="/registration-success" element={<RegistrationSuccess />} />
        <Route path="/password-reset-success" element={<PasswordResetSuccess />} />
        <Route path="/home" element={<h1>Home Dashboard (Coming Soon)</h1>} />
      </Routes>
    </Router>
  )
}

export default App
